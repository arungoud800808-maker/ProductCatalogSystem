using System.Diagnostics;

namespace ProductService.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public const string HeaderName = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        string correlationId;

        if (context.Request.Headers.TryGetValue(HeaderName, out var existing))
        {
            correlationId = existing!;
        }
        else
        {
            correlationId = Guid.NewGuid().ToString();
        }

        context.TraceIdentifier = correlationId;

        Activity.Current?.AddTag("CorrelationId", correlationId);

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        await _next(context);
    }
}