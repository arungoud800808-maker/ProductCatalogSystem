using System.Text;

namespace ProductService.Middleware;

public class SwaggerBasicAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public SwaggerBasicAuthMiddleware(
        RequestDelegate next,
        IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        string authHeader = context.Request.Headers["Authorization"];

        if (string.IsNullOrEmpty(authHeader) ||
            !authHeader.StartsWith("Basic "))
        {
            context.Response.Headers["WWW-Authenticate"] = "Basic";
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var encoded = authHeader.Substring("Basic ".Length).Trim();

        var credentialBytes = Convert.FromBase64String(encoded);

        var credentials = Encoding.UTF8.GetString(credentialBytes);

        var values = credentials.Split(':');

        if (values.Length != 2)
        {
            context.Response.StatusCode = 401;
            return;
        }

        var username = values[0];
        var password = values[1];

        if (username != _configuration["Swagger:Username"] ||
            password != _configuration["Swagger:Password"])
        {
            context.Response.StatusCode = 401;
            return;
        }

        await _next(context);
    }
}