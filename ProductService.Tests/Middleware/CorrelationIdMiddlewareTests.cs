using FluentAssertions;
using Microsoft.AspNetCore.Http;
using ProductService.Middleware;
using Xunit;

namespace ProductService.Tests.Middleware;

public class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task Invoke_WhenCorrelationIdExists_ShouldUseExistingCorrelationId()
    {
        // Arrange
        var expectedId = "test-correlation-id";

        RequestDelegate next = context =>
        {
            return Task.CompletedTask;
        };


        var middleware =
            new CorrelationIdMiddleware(next);


        var context =
            new DefaultHttpContext();


        context.Request.Headers[
            CorrelationIdMiddleware.HeaderName] = expectedId;



        // Act
        await middleware.Invoke(context);



        // Assert
        context.TraceIdentifier
            .Should()
            .Be(expectedId);
    }
    [Fact]
    public async Task Invoke_WhenCorrelationIdMissing_ShouldGenerateNewCorrelationId()
    {
        // Arrange
        var context = new DefaultHttpContext();

        var middleware =
            new CorrelationIdMiddleware(
                ctx => Task.CompletedTask);



        // Act
        await middleware.Invoke(context);



        // Assert
        context.TraceIdentifier
            .Should()
            .NotBeNullOrEmpty();

        Guid.TryParse(context.TraceIdentifier, out _)
            .Should()
            .BeTrue();
    }

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


        var middleware =
            new CorrelationIdMiddleware(next);


        var context =
            new DefaultHttpContext();



        // Act
        await middleware.Invoke(context);



        // Assert
        nextCalled.Should()
            .BeTrue();
    }



    [Fact]
    public async Task Invoke_ShouldSetTraceIdentifier()
    {
        // Arrange
        var context =
            new DefaultHttpContext();


        var middleware =
            new CorrelationIdMiddleware(
                ctx => Task.CompletedTask);



        // Act
        await middleware.Invoke(context);



        // Assert
        context.TraceIdentifier
            .Should()
            .NotBeNullOrEmpty();
    }
}