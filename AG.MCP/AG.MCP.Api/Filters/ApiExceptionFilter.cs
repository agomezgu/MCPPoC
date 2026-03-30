using AG.MCP.Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AG.MCP.Api.Filters;

public class ApiExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ApiExceptionFilter> _logger;

    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger) => _logger = logger;

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Unhandled exception");

        var (statusCode, code, message) = context.Exception switch
        {
            InvalidOperationException ex => (400, "BAD_REQUEST", ex.Message),
            KeyNotFoundException => (404, "NOT_FOUND", "Resource not found."),
            UnauthorizedAccessException => (401, "UNAUTHORIZED", "Access denied."),
            _ => (500, "INTERNAL_ERROR", "An unexpected error occurred.")
        };

        context.Result = new ObjectResult(new ApiError(code, message))
        {
            StatusCode = statusCode
        };
        context.ExceptionHandled = true;
    }
}
