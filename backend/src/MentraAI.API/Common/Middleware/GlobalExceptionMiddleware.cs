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
        // If SSE streaming has already started, we cannot write a JSON error —
        // the response is committed. Log and bail; the client sees the stream end abruptly.
        if (ctx.Response.HasStarted)
        {
            _logger.LogError(ex,
                "Exception after response started — {Method} {Path}",
                ctx.Request.Method, ctx.Request.Path);
            return;
        }

        var (status, code, message, errors) = ex switch
        {
            AppException a => (a.StatusCode, a.ErrorCode, a.Message, a.Errors),
            AIServiceException => (502, ErrorCodes.AI_INTERNAL_ERROR, "AI service error.", null),
            AIValidationException => (502, ErrorCodes.AI_RESPONSE_INVALID, "AI returned unexpected response.", null),
            OperationCanceledException when ctx.RequestAborted.IsCancellationRequested => (499, "CLIENT_CLOSED", "Client disconnected.", null),
            OperationCanceledException => (504, ErrorCodes.AI_TIMEOUT, "AI service timed out.", null),
            System.Net.Http.HttpRequestException => (502, ErrorCodes.AI_SERVICE_UNAVAILABLE, "AI service is unreachable.", null),
            UnauthorizedAccessException => (401, ErrorCodes.UNAUTHORIZED, "Authentication required.", null),
            _ => (500, ErrorCodes.INTERNAL_ERROR, "An unexpected error occurred.", null)
        };

        _logger.LogError(ex, "Request {Method} {Path} → {Code}",
            ctx.Request.Method, ctx.Request.Path, code);

        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json";

        await ctx.Response.WriteAsJsonAsync(new
        {
            success = false,
            error = new { code, message, statusCode = status, errors }
        });
    }
}