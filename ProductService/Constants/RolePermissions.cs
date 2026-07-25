using System.Collections.Generic;

namespace ProductService.Constants
{
    public static class RolePermissions
    {
        public static readonly Dictionary<string, List<string>> RolePermissionMap =
            new()
            {
                ["Admin"] = new()
                {
                    Permissions.ProductView,
                    Permissions.ProductCreate,
                    Permissions.ProductUpdate,
                    Permissions.ProductDelete,
                    Permissions.SwaggerView,

                    Permissions.CategoryView,
                    Permissions.CategoryCreate,
                    Permissions.CategoryUpdate,
                    Permissions.CategoryDelete,

                    Permissions.ReviewCreate,
                    Permissions.ReviewDelete,

                    Permissions.UserView,
                    Permissions.UserDelete,
                    Permissions.UserLock,
                    Permissions.UserUnlock,

                    Permissions.DashboardView,
                    Permissions.AuditView,

                    Permissions.WishlistManage
                },

                ["Manager"] = new()
                {
                    Permissions.ProductView,
                    Permissions.ProductCreate,
                    Permissions.ProductUpdate,

                    Permissions.CategoryView,
                    Permissions.CategoryCreate,
                    Permissions.CategoryUpdate,

                    Permissions.ReviewDelete,

                    Permissions.DashboardView
                },

                ["Customer"] = new()
                {
                    Permissions.ProductView,

                    Permissions.ReviewCreate,

                    Permissions.WishlistManage
                }
            };
    }
}