using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace AG.Mcp.Server.Ui;

[McpServerResourceType]
public static class UiResources
{
    public const string ClientsAppResourceUri = "ui://ag.mcp/clients";

    [McpServerResource(
        UriTemplate = ClientsAppResourceUri,
        Name = "clients-ui",
        Title = "Clients UI App",
        MimeType = "text/html;profile=mcp-app")]
    [Description("MCP App shell that renders the local Angular clients screen.")]
    public static ResourceContents ClientsUiResource(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var publicBaseUrl = GetPublicBaseUrl(configuration);
        var assetsBaseUrl = $"{publicBaseUrl}/clients-ui/browser/";
        var indexPath = Path.Combine(environment.WebRootPath, "clients-ui", "browser", "index.html");

        var html = File.Exists(indexPath)
            ? File.ReadAllText(indexPath, Encoding.UTF8)
            : "<!doctype html><html><body><p>Clients UI build not found. Run <code>npm run build:mcp</code>.</p></body></html>";

        // #region agent log
        DebugLog("ui-resource:before-rewrite", new
        {
            hypothesisId = "H-A,H-B,H-C",
            assetsBaseUrl,
            originalBaseTagFound = html.Contains("<base href=\"/clients-ui/browser/\">", StringComparison.Ordinal),
            originalHrefFound = html.Contains("href=\"/clients-ui/browser/", StringComparison.Ordinal),
            firstChars = html.Length > 400 ? html[..400] : html
        });
        // #endregion

        var browserDir = Path.Combine(environment.WebRootPath, "clients-ui", "browser");
        var inlinedStylesheetCount = 0;
        var inlinedScriptCount = 0;

        html = Regex.Replace(
            html,
            "<link\\s+rel=\"stylesheet\"\\s+href=\"/clients-ui/browser/(?<file>[^\"]+)\"\\s*/?>",
            match =>
            {
                var fileName = match.Groups["file"].Value;
                var filePath = Path.Combine(browserDir, fileName);
                if (!File.Exists(filePath))
                {
                    return match.Value;
                }

                inlinedStylesheetCount++;
                var css = File.ReadAllText(filePath, Encoding.UTF8);
                return $"<style>{css}</style>";
            },
            RegexOptions.IgnoreCase);

        // The MCP App iframe enforces script-src 'self' 'unsafe-inline'. Cross-origin
        // chunk loads are blocked, so we inline the single bundled script (produced
        // by scripts/bundle-mcp.mjs) and strip any modulepreload hints.
        html = Regex.Replace(
            html,
            "<link\\s+rel=\"modulepreload\"[^>]*>\\s*",
            string.Empty,
            RegexOptions.IgnoreCase);

        html = Regex.Replace(
            html,
            "<script\\s+src=\"/clients-ui/browser/(?<file>mcp-bundle\\.js)\"[^>]*></script>",
            match =>
            {
                var fileName = match.Groups["file"].Value;
                var filePath = Path.Combine(browserDir, fileName);
                if (!File.Exists(filePath))
                {
                    return match.Value;
                }

                inlinedScriptCount++;
                var js = File.ReadAllText(filePath, Encoding.UTF8);
                return $"<script>{js}</script>";
            },
            RegexOptions.IgnoreCase);

        html = html
            .Replace("<base href=\"/clients-ui/browser/\">", "<base href=\"./\">", StringComparison.Ordinal)
            .Replace("href=\"/clients-ui/browser/", $"href=\"{assetsBaseUrl}", StringComparison.Ordinal)
            .Replace("src=\"/clients-ui/browser/", $"src=\"{assetsBaseUrl}", StringComparison.Ordinal)
            .Replace("<app-root></app-root>", "<app-root></app-root><script>if (!location.hash) location.hash = '#/clients/agent-create';</script>", StringComparison.Ordinal);

        // #region agent log
        DebugLog("ui-resource:inlined-assets", new
        {
            hypothesisId = "H-F,H-G",
            inlinedStylesheetCount,
            inlinedScriptCount,
            hasRemoteScript = html.Contains("<script src=\"http://", StringComparison.Ordinal) || html.Contains("<script src=\"https://", StringComparison.Ordinal),
            hasModulePreload = html.Contains("modulepreload", StringComparison.Ordinal),
            hasRemoteStylesheet = html.Contains("<link rel=\"stylesheet\" href=\"http://", StringComparison.Ordinal)
        });
        // #endregion

        // #region agent log
        var baseTagIndex = html.IndexOf("<base", StringComparison.Ordinal);
        var baseTagSnippet = baseTagIndex >= 0
            ? html.Substring(baseTagIndex, Math.Min(120, html.Length - baseTagIndex))
            : "(no <base> tag)";
        DebugLog("ui-resource:after-rewrite", new
        {
            hypothesisId = "H-A,H-B,H-C,H-D",
            assetsBaseUrl,
            baseTagSnippet,
            firstChars = html.Length > 600 ? html[..600] : html
        });
        // #endregion

        return new TextResourceContents
        {
            Uri = ClientsAppResourceUri,
            MimeType = "text/html;profile=mcp-app",
            Text = html
        };
    }

    private static void DebugLog(string location, object data)
    {
        try
        {
            var entry = new
            {
                sessionId = "a2af49",
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                location,
                message = location,
                data
            };
            var line = JsonSerializer.Serialize(entry);
            File.AppendAllText(@"c:\AndresG\AICourses\RAGAzure\debug-a2af49.log", line + Environment.NewLine, Encoding.UTF8);
        }
        catch
        {
        }
    }

    private static string GetPublicBaseUrl(IConfiguration configuration)
    {
        var serverUrls = configuration["urls"] ?? configuration["ASPNETCORE_URLS"];
        var runtimeUrl = serverUrls?
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeLocalUrl)
            .FirstOrDefault(url => url.StartsWith("http://localhost", StringComparison.OrdinalIgnoreCase))
            ?? serverUrls?
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(NormalizeLocalUrl)
                .FirstOrDefault(url => url.StartsWith("http://", StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(runtimeUrl))
        {
            return runtimeUrl.TrimEnd('/');
        }

        var configuredBaseUrl = configuration["McpServer:PublicBaseUrl"];
        if (!string.IsNullOrWhiteSpace(configuredBaseUrl))
        {
            return configuredBaseUrl.TrimEnd('/');
        }

        return "http://localhost:7070";
    }

    private static string NormalizeLocalUrl(string url)
    {
        return url
            .Replace("http://*:", "http://localhost:", StringComparison.OrdinalIgnoreCase)
            .Replace("http://+:", "http://localhost:", StringComparison.OrdinalIgnoreCase);
    }
}
