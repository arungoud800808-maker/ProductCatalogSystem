using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using ProductService.Authorization;
using ProductService.Constants;
using System.Security.Claims;
using Xunit;

namespace ProductService.Tests.Authentication;

public class AuthorizationTests
{
    private readonly PermissionHandler _handler;

    public AuthorizationTests()
    {
        _handler = new PermissionHandler();
    }


    [Fact]
    public async Task Admin_With_ProductCreatePermission_ShouldSucceed()
    {
        // Arrange
        var user = CreateUser(
            Roles.Admin);

        var requirement =
            new PermissionRequirement(
                Permissions.ProductCreate);

        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            null);


        // Act
        await _handler.HandleAsync(context);


        // Assert
        context.HasSucceeded.Should().BeTrue();
    }



    [Fact]
    public async Task Manager_With_ProductUpdatePermission_ShouldSucceed()
    {
        // Arrange
        var user = CreateUser(
            Roles.Manager);

        var requirement =
            new PermissionRequirement(
                Permissions.ProductUpdate);

        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            null);


        // Act
        await _handler.HandleAsync(context);


        // Assert
        context.HasSucceeded.Should().BeTrue();
    }



    [Fact]
    public async Task Customer_With_ReviewCreatePermission_ShouldSucceed()
    {
        // Arrange
        var user = CreateUser(
            Roles.Customer);

        var requirement =
            new PermissionRequirement(
                Permissions.ReviewCreate);


        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            null);


        // Act
        await _handler.HandleAsync(context);


        // Assert
        context.HasSucceeded.Should().BeTrue();
    }



    [Fact]
    public async Task Customer_With_ProductDeletePermission_ShouldFail()
    {
        // Arrange
        var user = CreateUser(
            Roles.Customer);

        var requirement =
            new PermissionRequirement(
                Permissions.ProductDelete);


        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            null);


        // Act
        await _handler.HandleAsync(context);


        // Assert
        context.HasSucceeded.Should().BeFalse();
    }



    [Fact]
    public async Task User_WithoutRoleClaim_ShouldFail()
    {
        // Arrange
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(
                    ClaimTypes.Name,
                    "Test User")
            },
            "TestAuthentication");


        var user = new ClaimsPrincipal(identity);


        var requirement =
            new PermissionRequirement(
                Permissions.ProductView);


        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            null);



        // Act
        await _handler.HandleAsync(context);



        // Assert
        context.HasSucceeded.Should().BeFalse();
    }



    [Fact]
    public async Task AnonymousUser_ShouldFailAuthorization()
    {
        // Arrange
        var user =
            new ClaimsPrincipal(
                new ClaimsIdentity());


        var requirement =
            new PermissionRequirement(
                Permissions.ProductView);


        var context =
            new AuthorizationHandlerContext(
                new[] { requirement },
                user,
                null);



        // Act
        await _handler.HandleAsync(context);



        // Assert
        context.HasSucceeded.Should().BeFalse();
    }



    [Fact]
    public async Task Admin_With_InvalidPermission_ShouldFail()
    {
        // Arrange
        var user =
            CreateUser(Roles.Admin);


        var requirement =
            new PermissionRequirement(
                "Invalid.Permission");


        var context =
            new AuthorizationHandlerContext(
                new[] { requirement },
                user,
                null);



        // Act
        await _handler.HandleAsync(context);



        // Assert
        context.HasSucceeded.Should().BeFalse();
    }



    [Fact]
    public void PermissionRequirement_ShouldStorePermission()
    {
        // Arrange
        var permission =
            Permissions.ProductCreate;


        // Act
        var requirement =
            new PermissionRequirement(permission);


        // Assert
        requirement.Permission
            .Should()
            .Be(permission);
    }



    private ClaimsPrincipal CreateUser(string role)
    {
        var claims = new[]
        {
            new Claim(
                ClaimTypes.Role,
                role),

            new Claim(
                ClaimTypes.Name,
                "Test User")
        };


        var identity =
            new ClaimsIdentity(
                claims,
                "TestAuthentication");


        return new ClaimsPrincipal(identity);
    }
}