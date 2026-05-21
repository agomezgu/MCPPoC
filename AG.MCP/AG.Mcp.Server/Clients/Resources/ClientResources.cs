using System.ComponentModel;
using System.Text.Json;
using AG.Mcp.Server.InvoicingApi;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace AG.Mcp.Server.Clients.Resources;

[McpServerResourceType]
public static class ClientResources
{
    public const string ResourceClientsUri = "l&linvoicing://clients";
    public const string ResourceClientByIdUri = "l&linvoicing://clients/{clientId}";
    public const string ResourceClientSummaryUri = "l&linvoicing://clients/{clientId}/summary";

    [McpServerResource(
        UriTemplate = ResourceClientsUri,
        Name = "clients.json",
        Title = "Client List",
        MimeType = "application/json")]
    [Description("Provides a paginated list of clients in the l&l company invoicing system. Returns client id, name, tax ID, contact info, and active status.")]
    public static async Task<ResourceContents> ClientListResource(
        IInvoicingApiClient apiClient,
        CancellationToken cancellationToken)
    {
        var result = await apiClient.GetClientsAsync(page: 1, pageSize: 50, ct: cancellationToken);

        return new TextResourceContents
        {
            Text = JsonSerializer.Serialize(result, McpJsonUtilities.DefaultOptions),
            MimeType = "application/json",
            Uri = ResourceClientsUri,
        };
    }

    [McpServerResource(
        UriTemplate = ResourceClientByIdUri,
        Name = "Client by ID",
        Title = "Client Details",
        MimeType = "application/json")]
    [Description("Retrieves a specific client by ID (GUID) from the l&l invoicing system. Returns full client details including name, tax ID, email, phone, and address.")]
    public static async Task<ResourceContents> ClientByIdResource(
        string clientId,
        IInvoicingApiClient apiClient,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(clientId, out var id))
            throw new McpProtocolException($"Invalid client ID: {clientId}", McpErrorCode.InvalidRequest);

        var client = await apiClient.GetClientAsync(id, cancellationToken) ?? throw new McpProtocolException($"Client not found: {clientId}", McpErrorCode.InvalidRequest);
        return new TextResourceContents
        {
            Text = JsonSerializer.Serialize(client, McpJsonUtilities.DefaultOptions),
            MimeType = "application/json",
            Uri = ResourceClientByIdUri.Replace("{clientId}", clientId),
        };
    }

    [McpServerResource(
        UriTemplate = ResourceClientSummaryUri,
        Name = "Client Financial Summary",
        Title = "Client Summary",
        MimeType = "application/json")]
    [Description("Retrieves the financial summary for a client of l&l Company. Includes total invoices, pending/overdue counts, total billed, total paid, total pending, and total overdue amounts.")]
    public static async Task<ResourceContents> ClientSummaryResource(
        string clientId,
        IInvoicingApiClient apiClient,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(clientId, out var id))
            throw new McpProtocolException($"Invalid client ID: {clientId}", McpErrorCode.InvalidRequest);

        var summary = await apiClient.GetClientSummaryAsync(id, cancellationToken);

        if (summary is null)
            throw new McpProtocolException($"Client not found: {clientId}", McpErrorCode.InvalidRequest);

        return new TextResourceContents
        {
            Text = JsonSerializer.Serialize(summary, McpJsonUtilities.DefaultOptions),
            MimeType = "application/json",
            Uri = ResourceClientSummaryUri.Replace("{clientId}", clientId),
        };
    }
}
