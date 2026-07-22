using Microsoft.AspNetCore.Authorization;
using ProductService.Constants;
using System.Security.Claims;

namespace ProductService.Authorization
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            // User must be authenticated
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                return Task.CompletedTask;
            }

            // Get user's role from JWT
            var role = context.User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(role))
            {
                return Task.CompletedTask;
            }

            // Check if the role has the required permission
            if (RolePermissions.RolePermissionMap.TryGetValue(role, out var permissions))
            {
                if (permissions.Contains(requirement.Permission))
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}