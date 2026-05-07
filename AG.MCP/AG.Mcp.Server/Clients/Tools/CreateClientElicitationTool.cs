using System.ComponentModel;
using System.Text.Json;
using AG.Mcp.Server.InvoicingApi;
using AG.Mcp.Server.InvoicingApi.Models;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using static ModelContextProtocol.Protocol.ElicitRequestParams;

namespace AG.Mcp.Server.Clients.Tools;

[McpServerToolType]
public sealed class CreateClientElicitationTool
{
    private readonly IInvoicingApiClient _apiClient;

    public CreateClientElicitationTool(IInvoicingApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [McpServerTool(Name = "create_client_elicit", Title = "Create Client (elicit missing fields)"),
     Description(
        "Default tool for creating a client in axxbeggs Company from chat. " +
        "Call this whenever a user asks to create/onboard a client, even if only a name is known. " +
        "Required fields are 'name' and 'taxId'; any missing fields are collected via an MCP elicitation form. " +
        "Prefer this over 'create_client' for any conversational flow.")]
    public async Task<ClientDto> CreateClientWithElicitation(
        McpServer server,
        [Description("Client display name")] string? name = null,
        [Description("Tax identification number")] string? taxId = null,
        [Description("Contact email")] string? email = null,
        [Description("Contact phone")] string? phone = null,
        [Description("Postal address")] string? address = null,
        CancellationToken cancellationToken = default)
    {
        var initialName = NormalizeOptional(name);
        var initialTaxId = NormalizeOptional(taxId);
        var initialEmail = NormalizeOptional(email);
        var initialPhone = NormalizeOptional(phone);
        var initialAddress = NormalizeOptional(address);

        if (!string.IsNullOrWhiteSpace(initialName) && !string.IsNullOrWhiteSpace(initialTaxId))
        {
            return await _apiClient.CreateClientAsync(
                new CreateClientRequest(initialName!, initialTaxId!, initialEmail, initialPhone, initialAddress),
                cancellationToken);
        }

        if (server.ClientCapabilities?.Elicitation?.Form is null)
        {
            throw new McpException(
                "Name and tax ID are required to create a client. Provide both tool arguments, or use an MCP client that supports form elicitation so missing fields can be collected.");
        }

        var schema = new RequestSchema
        {
            Properties = new Dictionary<string, PrimitiveSchemaDefinition>
            {
                ["name"] = new StringSchema
                {
                    Description = "Client display name",
                    Default = initialName ?? ""
                },
                ["taxId"] = new StringSchema
                {
                    Description = "Tax identification number",
                    Default = initialTaxId ?? ""
                },
                ["email"] = new StringSchema
                {
                    Description = "Contact email (optional)",
                    Default = initialEmail ?? ""
                },
                ["phone"] = new StringSchema
                {
                    Description = "Contact phone (optional)",
                    Default = initialPhone ?? ""
                },
                ["address"] = new StringSchema
                {
                    Description = "Postal address (optional)",
                    Default = initialAddress ?? ""
                }
            }
        };

        var result = await server.ElicitAsync(
            new ElicitRequestParams
            {
                Message = "Provide or confirm client details to complete creation.",
                RequestedSchema = schema
            },
            cancellationToken);

        if (!string.Equals(result.Action, "accept", StringComparison.OrdinalIgnoreCase))
        {
            throw new McpException("Client creation was cancelled or declined.");
        }

        var content = result.Content;
        if (content is null)
        {
            throw new McpException("Elicitation completed without form content.");
        }

        var finalName = Coalesce(GetString(content, "name"), initialName);
        var finalTaxId = Coalesce(GetString(content, "taxId"), initialTaxId);
        var finalEmail = Coalesce(GetString(content, "email"), initialEmail);
        var finalPhone = Coalesce(GetString(content, "phone"), initialPhone);
        var finalAddress = Coalesce(GetString(content, "address"), initialAddress);

        if (string.IsNullOrWhiteSpace(finalName) || string.IsNullOrWhiteSpace(finalTaxId))
        {
            throw new McpException("Name and tax ID are required after elicitation.");
        }

        return await _apiClient.CreateClientAsync(
            new CreateClientRequest(finalName!, finalTaxId!, finalEmail, finalPhone, finalAddress),
            cancellationToken);
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>Prefer non-empty form value; otherwise use the initial argument.</summary>
    private static string? Coalesce(string? fromForm, string? initial) =>
        !string.IsNullOrWhiteSpace(fromForm) ? fromForm.Trim() : initial;

    private static string? GetString(IDictionary<string, JsonElement> dict, string key) =>
        dict.TryGetValue(key, out var el) && el.ValueKind == JsonValueKind.String ? el.GetString() : null;
}
