namespace Mind_Manager.Api.Middlewares;

using Mind_Manager.Api.Middlewares.ExceptionHandlers;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IEnumerable<IExceptionHandler> _handlers;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next, 
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IEnumerable<IExceptionHandler> handlers)
    {
        _next = next;
        _logger = logger;
        _handlers = handlers;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exceção não tratada: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var handler = _handlers.FirstOrDefault(h => h.CanHandle(exception));
        
        if (handler != null)
        {
            var (statusCode, response) = handler.Handle(exception);
            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(response);
        }
        else
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new ErrorResponse
            {
                Message = "Erro interno do servidor.",
                ErrorCode = "INTERNAL_ERROR",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
