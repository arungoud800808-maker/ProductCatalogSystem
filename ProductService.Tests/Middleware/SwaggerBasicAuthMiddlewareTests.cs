using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ProductService.Middleware;
using Xunit;

namespace ProductService.Tests.Middleware;

public class SwaggerBasicAuthMiddlewareTests
{
    private readonly IConfiguration _configuration;


    public SwaggerBasicAuthMiddlewareTests()
    {
        var settings = new Dictionary<string, string>
        {
            {
                "Swagger:Username",
                "admin"
            },
            {
                "Swagger:Password",
                "password123"
            }
        };


        _configuration =
            new ConfigurationBuilder()
                .AddInMemoryCollection(settings!)
                .Build();
    }



    [Fact]
    public async Task InvokeAsync_NonSwaggerRequest_ShouldCallNext()
    {
        // Arrange
        bool nextCalled = false;


        RequestDelegate next = context =>
        {
            nextCalled = true;

            return Task.CompletedTask;
        };


        var middleware =
            new SwaggerBasicAuthMiddleware(
                next,
                _configuration);


        var context =
            new DefaultHttpContext();


        context.Request.Path = "/api/products";



        // Act
        await middleware.InvokeAsync(context);



        // Assert
        nextCalled.Should()
            .BeTrue();
    }




    [Fact]
    public async Task InvokeAsync_SwaggerRequestWithoutAuth_ShouldReturn401()
    {
        // Arrange
        RequestDelegate next = context =>
        {
            return Task.CompletedTask;
        };


        var middleware =
            new SwaggerBasicAuthMiddleware(
                next,
                _configuration);


        var context =
            new DefaultHttpContext();


        context.Request.Path = "/swagger/index.html";



        // Act
        await middleware.InvokeAsync(context);



        // Assert
        context.Response.StatusCode
            .Should()
            .Be(StatusCodes.Status401Unauthorized);


        context.Response.Headers["WWW-Authenticate"]
            .ToString()
            .Should()
            .Be("Basic");
    }




    [Fact]
    public async Task InvokeAsync_WithInvalidCredentials_ShouldReturn401()
    {
        // Arrange
        var invalidCredentials =
            Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(
                    "admin:wrongpassword"));


        RequestDelegate next = context =>
        {
            return Task.CompletedTask;
        };


        var middleware =
            new SwaggerBasicAuthMiddleware(
                next,
                _configuration);


        var context =
            new DefaultHttpContext();


        context.Request.Path =
            "/swagger/index.html";


        context.Request.Headers["Authorization"] =
            $"Basic {invalidCredentials}";



        // Act
        await middleware.InvokeAsync(context);



        // Assert
        context.Response.StatusCode
            .Should()
            .Be(StatusCodes.Status401Unauthorized);
    }




    [Fact]
    public async Task InvokeAsync_WithValidCredentials_ShouldCallNext()
    {
        // Arrange
        bool nextCalled = false;


        var credentials =
            Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(
                    "admin:password123"));


        RequestDelegate next = context =>
        {
            nextCalled = true;

            return Task.CompletedTask;
        };


        var middleware =
            new SwaggerBasicAuthMiddleware(
                next,
                _configuration);


        var context =
            new DefaultHttpContext();


        context.Request.Path =
            "/swagger/index.html";


        context.Request.Headers["Authorization"] =
            $"Basic {credentials}";



        // Act
        await middleware.InvokeAsync(context);



        // Assert
        nextCalled.Should()
            .BeTrue();
    }




    [Fact]
    public async Task InvokeAsync_WithMalformedCredentials_ShouldReturn401()
    {
        // Arrange
        var invalidCredentials =
            Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(
                    "admin"));


        var middleware =
            new SwaggerBasicAuthMiddleware(
                context => Task.CompletedTask,
                _configuration);


        var context =
            new DefaultHttpContext();


        context.Request.Path =
            "/swagger/index.html";


        context.Request.Headers["Authorization"] =
            $"Basic {invalidCredentials}";



        // Act
        await middleware.InvokeAsync(context);



        // Assert
        context.Response.StatusCode
            .Should()
            .Be(401);
    }
}