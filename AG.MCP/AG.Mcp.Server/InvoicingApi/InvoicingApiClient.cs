using AG.MCP.Application.Common;
using AG.MCP.Application.DTOs;
using Microsoft.Extensions.Options;
using RestEase;

namespace AG.Mcp.Server.InvoicingApi;

public class InvoicingApiClient : IInvoicingApiClient
{
    public readonly HttpClient _httpClient;
    public readonly IInvoicingApiRestClient _restClient;

    public InvoicingApiClient(HttpClient httpClient, IOptions<InvoicingApiOptions> options)
    {
        _httpClient = httpClient;
        var apiOptions = options.Value;
        
        _httpClient.BaseAddress = new Uri(apiOptions.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(apiOptions.TimeoutSeconds);
        
        _restClient = RestClient.For<IInvoicingApiRestClient>(_httpClient);
    }

    public async Task<PagedResult<ClientDto>> GetClientsAsync(
        int page,
        int pageSize,
        string? search = null,
        string? sortBy = null,
        bool sortDescending = false,
        CancellationToken ct = default)
    {
        return await _restClient.GetClientsAsync(page, pageSize, search, sortBy, sortDescending, ct);
    }

    public async Task<ClientDto?> GetClientAsync(Guid id, CancellationToken ct = default)
    {
        return await _restClient.GetClientAsync(id, ct);
    }

    public async Task<ClientDto> CreateClientAsync(CreateClientRequest request, CancellationToken ct = default)
    {
        return await _restClient.CreateClientAsync(request, ct);
    }

    public async Task<ClientDto?> UpdateClientAsync(Guid id, UpdateClientRequest request, CancellationToken ct = default)
    {
        return await _restClient.UpdateClientAsync(id, request, ct);
    }

    public async Task<ClientSummaryDto?> GetClientSummaryAsync(Guid id, CancellationToken ct = default)
    {
        return await _restClient.GetClientSummaryAsync(id, ct);
    }

    /// <summary>
    /// Internal RestEase interface for HTTP communication
    /// </summary>
    [BasePath("api/clients")]
    public interface IInvoicingApiRestClient
    {
        [Get("")]
        Task<PagedResult<ClientDto>> GetClientsAsync(
            [Query] int page,
            [Query] int pageSize,
            [Query] string? search = null,
            [Query] string? sortBy = null,
            [Query] bool sortDescending = false,
            CancellationToken ct = default);

        [Get("{id}")]
        Task<ClientDto?> GetClientAsync([Path] Guid id, CancellationToken ct = default);

        [Post("")]
        Task<ClientDto> CreateClientAsync([Body] CreateClientRequest request, CancellationToken ct = default);

        [Put("{id}")]
        Task<ClientDto?> UpdateClientAsync([Path] Guid id, [Body] UpdateClientRequest request, CancellationToken ct = default);

        [Get("{id}/summary")]
        Task<ClientSummaryDto?> GetClientSummaryAsync([Path] Guid id, CancellationToken ct = default);
    }
}
