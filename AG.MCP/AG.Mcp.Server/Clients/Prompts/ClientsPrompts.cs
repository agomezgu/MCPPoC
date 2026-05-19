using System.ComponentModel;
using System.Reflection;
using ModelContextProtocol.Server;

namespace AG.Mcp.Server.Clients.Prompts;

[McpServerPromptType]
public static class ClientsPrompts
{
    private static readonly Assembly Assembly = typeof(ClientsPrompts).Assembly;

    [McpServerPrompt(Title = "Client Financial Analysis Guide", Name = "Client Financial Analysis Guide")]
    [Description("Guide for analyzing client financial summaries, invoices, payments, and balances in the invoicing system.")]
    public static string ClientFinancialAnalysisPrompt() => LoadMarkdown("client-financial-analysis.md");

    [McpServerPrompt(Title = "Client Lookup Guide", Name = "Client Lookup Guide")]
    [Description("Guide for finding and retrieving client information by ID or search criteria.")]
    public static string ClientLookupPrompt() => LoadMarkdown("client-lookup.md");

    [McpServerPrompt(Title = "Client Onboarding Guide", Name = "Client Onboarding Guide")]
    [Description("Guide for creating and setting up new clients with validated information.")]
    public static string ClientOnboardingPrompt() => LoadMarkdown("client-onboarding.md");

    [McpServerPrompt(Title = "Client List Management Guide", Name = "Client List Management Guide")]
    [Description("Guide for browsing, filtering, and managing client lists.")]
    public static string ClientListManagementPrompt() => LoadMarkdown("client-list-management.md");

    [McpServerPrompt(Title = "Client Update Guide", Name = "Client Update Guide")]
    [Description("Guide for modifying and maintaining existing client information.")]
    public static string ClientUpdatePrompt() => LoadMarkdown("client-update.md");

    [McpServerPrompt(Title = "Client Validation Guide", Name = "Client Validation Guide")]
    [Description("Verify client data before creating or updating: tax ID validation, contact info, and duplicate checks.")]
    public static string ClientValidationPrompt() => LoadMarkdown("client-validation.md");

    [McpServerPrompt(Title = "Client Search Guide", Name = "Client Search Guide")]
    [Description("Search clients by name, tax ID, or other criteria with sorting and pagination.")]
    public static string ClientSearchPrompt() => LoadMarkdown("client-search.md");

    [McpServerPrompt(Title = "Client Exact Name Search", Name = "Client Exact Name Search")]
    [Description("Search clients by an exact Name value and ignore partial or non-name matches.")]
    public static string ClientExactNameSearchPrompt(
        [Description("Exact client Name to search for.")]
        string name)
    {
        var exactName = name.Trim();
        if (string.IsNullOrWhiteSpace(exactName))
            return "Ask the user for the exact client Name before searching.";

        return $"""
            Find clients whose `Name` exactly equals "{exactName}".

            Use `GetClients` with `search` set to "{exactName}", `page` set to 1, `pageSize` set to 50, and `sortBy` set to "name".
            The search endpoint performs a case-insensitive contains search across name, tax ID, and email, so only treat results as matches when the returned `Name` exactly equals "{exactName}".

            If the first page does not prove there are no more candidates, continue through the remaining pages and keep only exact `Name` matches.
            Report each exact match with `Id`, `Name`, `TaxId`, `Email`, and `IsActive`.
            If no exact match exists, say that no client was found with exactly that `Name` and do not present partial, tax ID, or email matches as results.
            """;
    }

    [McpServerPrompt(Title = "Bulk Client Operations Guide", Name = "Bulk Client Operations Guide")]
    [Description("Guide for performing operations on multiple clients using list, pagination, and repeated create or update calls.")]
    public static string BulkClientOperationsPrompt() => LoadMarkdown("bulk-client-operations.md");

    private static string LoadMarkdown(string fileName)
    {
        var stream = Assembly.GetManifestResourceStream($"{typeof(ClientsPrompts).Namespace}.{fileName}");
        if (stream is not null)
        {
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        var match = Assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));
        if (match is null)
            throw new InvalidOperationException(
                $"Embedded prompt not found: {fileName}. Known resources: {string.Join(", ", Assembly.GetManifestResourceNames())}");

        using var r = new StreamReader(Assembly.GetManifestResourceStream(match)!);
        return r.ReadToEnd();
    }
}
