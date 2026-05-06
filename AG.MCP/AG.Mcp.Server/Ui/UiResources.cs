using System.ComponentModel;
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
    public static ResourceContents ClientsUiResource(IConfiguration configuration)
    {
        var baseUrl = configuration["AngularUi:BaseUrl"] ?? "http://localhost:4200";
        var clientsUrl = $"{baseUrl.TrimEnd('/')}/clients";

        var html = $$"""
            <!doctype html>
            <html lang="en">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1" />
              <title>AG MCP Clients UI</title>
              <style>
                html, body {
                  margin: 0;
                  padding: 0;
                  width: 100%;
                  height: 100%;
                  background: #111827;
                }
                iframe {
                  border: 0;
                  width: 100%;
                  height: 100%;
                }
              </style>
            </head>
            <body>
              <iframe src="{{clientsUrl}}" title="AG MCP Clients UI"></iframe>
            </body>
            </html>
            """;

        return new TextResourceContents
        {
            Uri = ClientsAppResourceUri,
            MimeType = "text/html;profile=mcp-app",
            Text = html
        };
    }
}
