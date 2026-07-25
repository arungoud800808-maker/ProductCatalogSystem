using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using ProductService.Middleware;
using Xunit;

namespace ProductService.Tests.Middleware;

public class RequestTracingMiddlewareTests
{
    [Fact]
    public async Task Invoke_ShouldCallNextMiddleware()
    {
        // Arrange
        bool nextCalled = false;


        RequestDelegate next = context =>
        {
            nextCalled = true;

            return Task.CompletedTask;
        };


        var loggerMock =
            new Mock<ILogger<RequestTracingMiddleware>>();


        var middleware =
            new RequestTracingMiddleware(
                next,
                loggerMock.Object);


        var context =
            new DefaultHttpContext();



        // Act
        await middleware.Invoke(context);



        // Assert
        nextCalled.Should()
            .BeTrue();
    }



    [Fact]
    public async Task Invoke_ShouldLogRequestStarted()
    {
        // Arrange
        RequestDelegate next = context =>
        {
            return Task.CompletedTask;
        };


        var loggerMock =
            new Mock<ILogger<RequestTracingMiddleware>>();


        var middleware =
            new RequestTracingMiddleware(
                next,
                loggerMock.Object);


        var context =
            new DefaultHttpContext();


        context.Request.Method = "GET";
        context.Request.Path = "/api/products";



        // Act
        await middleware.Invoke(context);



        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(
                    (v, t) =>
                        v.ToString()!
                         .Contains("Request Started")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }



    [Fact]
    public async Task Invoke_ShouldLogRequestFinished()
    {
        // Arrange
        RequestDelegate next = context =>
        {
            context.Response.StatusCode = 200;

            return Task.CompletedTask;
        };


        var loggerMock =
            new Mock<ILogger<RequestTracingMiddleware>>();


        var middleware =
            new RequestTracingMiddleware(
                next,
                loggerMock.Object);


        var context =
            new DefaultHttpContext();


        context.Request.Method = "POST";
        context.Request.Path = "/api/products";



        // Act
        await middleware.Invoke(context);



        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(
                    (v, t) =>
                        v.ToString()!
                         .Contains("Request Finished")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }



    [Fact]
    public async Task Invoke_ShouldPreserveResponseStatusCode()
    {
        // Arrange
        RequestDelegate next = context =>
        {
            context.Response.StatusCode = 404;

            return Task.CompletedTask;
        };


        var loggerMock =
            new Mock<ILogger<RequestTracingMiddleware>>();


        var middleware =
            new RequestTracingMiddleware(
                next,
                loggerMock.Object);


        var context =
            new DefaultHttpContext();



        // Act
        await middleware.Invoke(context);



        // Assert
        context.Response.StatusCode
            .Should()
            .Be(404);
    }
}