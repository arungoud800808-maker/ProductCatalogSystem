using System.Diagnostics;

namespace ProductService.Middleware;

public class RequestTracingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTracingMiddleware> _logger;

    public RequestTracingMiddleware(
        RequestDelegate next,
        ILogger<RequestTracingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Request Started: {Method} {Path}",
            context.Request.Method,
            context.Request.Path);

        await _next(context);

        stopwatch.Stop();

        _logger.LogInformation(
            "Request Finished: {Method} {Path} Status:{StatusCode} Duration:{Elapsed}ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }
}