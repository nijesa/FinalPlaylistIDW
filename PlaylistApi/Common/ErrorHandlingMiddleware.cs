namespace PlaylistApi.Common;

// Atrapa las HttpException y responde con el mismo formato de error que NestJS:
// { "statusCode": 404, "message": "Canción 5 no encontrada", "error": "Not Found" }
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (HttpException ex)
        {
            await WriteError(context, ex.StatusCode, ex.Message, ex.ErrorName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado");
            await WriteError(context, 500, "Internal server error", "Internal Server Error");
        }
    }

    private static Task WriteError(HttpContext context, int status, string message, string error)
    {
        if (context.Response.HasStarted) return Task.CompletedTask;

        context.Response.StatusCode = status;
        return context.Response.WriteAsJsonAsync(new { statusCode = status, message, error });
    }
}
