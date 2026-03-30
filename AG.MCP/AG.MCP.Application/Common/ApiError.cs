namespace AG.MCP.Application.Common;

public record ApiError(
    string Code,
    string Message,
    IDictionary<string, string[]>? Errors = null);
