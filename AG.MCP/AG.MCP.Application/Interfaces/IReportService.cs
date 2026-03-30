using AG.MCP.Application.DTOs;

namespace AG.MCP.Application.Interfaces;

public interface IReportService
{
    Task<AccountsReceivableSummaryDto> GetAccountsReceivableAsync(CancellationToken ct = default);
    Task<ClientStatementDto?> GetClientStatementAsync(Guid clientId, DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
    Task<SalesSummaryDto> GetSalesSummaryAsync(DateTime? fromDate, DateTime? toDate, CancellationToken ct = default);
}
