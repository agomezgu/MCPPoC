using System.ComponentModel;
using AG.Mcp.Server.InvoicingApi;
using AG.Mcp.Server.InvoicingApi.Models;
using ModelContextProtocol.Server;

namespace AG.Mcp.Server.Clients.Tools;

[McpServerToolType]
public class ClientsTool
{
    private readonly IInvoicingApiClient _apiClient;

    public ClientsTool(IInvoicingApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [McpServerTool, Description("Get paginated list of  clients of axxbeggs Company with optional search and sorting")]
    public async Task<PagedResult<ClientDto>> GetClients(
        [Description("Page number to retrieve")][DefaultValue(1)] int page = 1,
        [Description("Page size")][DefaultValue(20)] int pageSize = 20,
        [Description("Optional search text")][DefaultValue(null)] string? search = null,
        [Description("Optional sort field")][DefaultValue(null)] string? sortBy = null,
        [Description("Sort descending")][DefaultValue(false)] bool sortDescending = false,
        CancellationToken ct = default)
    {
        return await _apiClient.GetClientsAsync(page, pageSize, search, sortBy, sortDescending, ct);
    }

    [McpServerTool, Description("Get client by id of the  axxbeggs Company")]
    public async Task<ClientDto?> GetClientById(
        [Description("Client ID")]
        Guid id,
        CancellationToken ct = default)
    {
        return await _apiClient.GetClientAsync(id, ct);
    }

    [McpServerTool, Description(
        "Programmatic client creation for axxbeggs Company. Requires ALL fields (name, taxId, email, phone, address) up-front. " +
        "Do NOT use for conversational/chat creation; use 'create_client_elicit' instead so missing fields can be collected via a form.")]
    public async Task<ClientDto> CreateClient(
        [Description("Create client request with all fields populated")]
        CreateClientRequest request,
        CancellationToken ct = default)
    {
        return await _apiClient.CreateClientAsync(request, ct);
    }

    [McpServerTool, Description("Update an existing client of  axxbeggs Company")]
    public async Task<ClientDto?> UpdateClient(
        [Description("Client ID")]
        Guid id,
        [Description("Update client request")]
        UpdateClientRequest request,
        CancellationToken ct = default)
    {
        return await _apiClient.UpdateClientAsync(id, request, ct);
    }

    [McpServerTool, Description("Get financial summary for a  axxbeggs Company client ")]
    public async Task<ClientSummaryDto?> GetClientSummary(
        [Description("Client ID")]
        Guid id,
        CancellationToken ct = default)
    {
        return await _apiClient.GetClientSummaryAsync(id, ct);
    }
}
