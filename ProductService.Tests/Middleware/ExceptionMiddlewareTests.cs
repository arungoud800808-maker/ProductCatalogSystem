using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using ProductService.Middleware;
using System.Net;
using System.Text.Json;
using Xunit;

namespace ProductService.Tests.Middleware;

public class ExceptionMiddlewareTests
{
    [Fact]
    public async Task Invoke_WhenNoException_ShouldCallNextMiddleware()
    {
        // Arrange
        var nextCalled = false;

        RequestDelegate next = context =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var loggerMock = new Mock<ILogger<ExceptionMiddleware>>();

        var middleware = new ExceptionMiddleware(
            next,
            loggerMock.Object);


        var context = new DefaultHttpContext();


        // Act
        await middleware.Invoke(context);


        // Assert
        nextCalled.Should().BeTrue();

        context.Response.StatusCode
            .Should()
            .Be((int)HttpStatusCode.OK);
    }



    [Fact]
    public async Task Invoke_WhenExceptionOccurs_ShouldReturn500Response()
    {
        // Arrange
        RequestDelegate next = context =>
        {
            throw new Exception("Test exception");
        };


        var loggerMock = new Mock<ILogger<ExceptionMiddleware>>();

        var middleware = new ExceptionMiddleware(
            next,
            loggerMock.Object);


        var context = new DefaultHttpContext();


        using var responseBody = new MemoryStream();

        context.Response.Body = responseBody;



        // Act
        await middleware.Invoke(context);



        // Assert
        context.Response.StatusCode
            .Should()
            .Be((int)HttpStatusCode.InternalServerError);


        context.Response.ContentType
            .Should()
            .Contain("application/json");


        responseBody.Position = 0;

        var responseText =
            await new StreamReader(responseBody)
                .ReadToEndAsync();


        var json =
            JsonSerializer.Deserialize<JsonElement>(
                responseText);


        json.GetProperty("StatusCode")
            .GetInt32()
            .Should()
            .Be(500);


        json.GetProperty("Message")
            .GetString()
            .Should()
            .Be("Test exception");
    }



    [Fact]
    public async Task Invoke_WhenExceptionOccurs_ShouldLogError()
    {
        // Arrange
        RequestDelegate next = context =>
        {
            throw new Exception("Database failed");
        };


        var loggerMock =
            new Mock<ILogger<ExceptionMiddleware>>();


        var middleware =
            new ExceptionMiddleware(
                next,
                loggerMock.Object);


        var context =
            new DefaultHttpContext();



        // Act
        await middleware.Invoke(context);



        // Assert
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!
                     .Contains("Database failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}