namespace App.Middlewares;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public Task InvokeAsync(HttpContext context)
    {
        logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path.Value}");
        return next(context);
    }
}