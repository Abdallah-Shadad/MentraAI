using MentraAI.API.Common.Errors;
using MentraAI.API.Common.Exceptions;

namespace MentraAI.API.Common.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (Exception ex)
        {
            await HandleAsync(ctx, ex);
        }
    }

    private async Task HandleAsync(HttpContext ctx, Exception ex)
    {
        var (status, code, message) = ex switch
        {
            AppException a => (a.StatusCode, a.ErrorCode, a.Message),
            AIServiceException => (502, ErrorCodes.AI_INTERNAL_ERROR, "AI service error."),
            AIValidationException => (502, ErrorCodes.AI_RESPONSE_INVALID, "AI returned unexpected response."),
            TaskCanceledException => (504, ErrorCodes.AI_TIMEOUT, "AI service timed out."),
            UnauthorizedAccessException => (401, ErrorCodes.UNAUTHORIZED, "Authentication required."),
            _ => (500, ErrorCodes.INTERNAL_ERROR, "An unexpected error occurred.")
        };

        _logger.LogError(ex, "Request {Method} {Path} → {Code}",
            ctx.Request.Method, ctx.Request.Path, code);

        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json";

        await ctx.Response.WriteAsJsonAsync(new
        {
            success = false,
            error = new { code, message, statusCode = status }
        });
    }
}