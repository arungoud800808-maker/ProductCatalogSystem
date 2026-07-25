using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProductService.Controllers;
using ProductService.DTOs;
using ProductService.Services;
using Xunit;

namespace ProductService.Tests.Controllers;

public class WishlistControllerTests
{
    private readonly Mock<IWishlistService> _serviceMock;
    private readonly WishlistController _controller;

    public WishlistControllerTests()
    {
        _serviceMock = new Mock<IWishlistService>();

        _controller = new WishlistController(_serviceMock.Object);
    }

    private void SetUser(int userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };

        var identity = new ClaimsIdentity(claims);

        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }

    [Fact]
    public async Task Add_ShouldReturnOk()
    {
        // Arrange

        SetUser(1);

        var dto = new CreateWishlistDto
        {
            ProductId = 10
        };

        _serviceMock
            .Setup(x => x.AddAsync(1, dto))
            .Returns(Task.CompletedTask);

        // Act

        var result = await _controller.Add(dto);

        // Assert

        var ok = result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        ok.StatusCode.Should().Be(StatusCodes.Status200OK);

        ok.Value.Should().Be("Product added to wishlist.");

        _serviceMock.Verify(
            x => x.AddAsync(1, dto),
            Times.Once);
    }

    [Fact]
    public async Task Add_WhenUserNotLoggedIn_ShouldReturnUnauthorized()
    {
        // Arrange

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        var dto = new CreateWishlistDto
        {
            ProductId = 10
        };

        // Act

        var result = await _controller.Add(dto);

        // Assert

        result.Should().BeOfType<UnauthorizedResult>();

        _serviceMock.Verify(
            x => x.AddAsync(It.IsAny<int>(), It.IsAny<CreateWishlistDto>()),
            Times.Never);
    }

    [Fact]
    public async Task Get_ShouldReturnWishlist()
    {
        // Arrange

        SetUser(1);

        var wishlist = new List<WishlistDto>
        {
            new WishlistDto
            {
                Id = 1
            },
            new WishlistDto
            {
                Id = 2
            }
        };

        _serviceMock
            .Setup(x => x.GetByUserAsync(1))
            .ReturnsAsync(wishlist);

        // Act

        var result = await _controller.Get();

        // Assert

        var ok = result.Should()
            .BeOfType<OkObjectResult>()
            .Subject;

        ok.StatusCode.Should().Be(StatusCodes.Status200OK);

        var value = ok.Value.Should()
            .BeAssignableTo<IEnumerable<WishlistDto>>()
            .Subject;

        value.Should().HaveCount(2);

        _serviceMock.Verify(
            x => x.GetByUserAsync(1),
            Times.Once);
    }

    [Fact]
    public async Task Get_WhenUserNotLoggedIn_ShouldReturnUnauthorized()
    {
        // Arrange

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act

        var result = await _controller.Get();

        // Assert

        result.Should().BeOfType<UnauthorizedResult>();

        _serviceMock.Verify(
            x => x.GetByUserAsync(It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent()
    {
        // Arrange

        _serviceMock
            .Setup(x => x.RemoveAsync(1))
            .Returns(Task.CompletedTask);

        // Act

        var result = await _controller.Delete(1);

        // Assert

        result.Should().BeOfType<NoContentResult>();

        _serviceMock.Verify(
            x => x.RemoveAsync(1),
            Times.Once);
    }
}