namespace AG.MCP.Application.Common;

public record QueryParams(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    string? SortBy = null,
    bool SortDescending = false,
    Guid? ClientId = null);
