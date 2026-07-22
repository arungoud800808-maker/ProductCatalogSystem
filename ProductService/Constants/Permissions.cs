namespace ProductService.Constants
{
    public static class Permissions
    {
        // Product
        public const string ProductView = "Product.View";
        public const string ProductCreate = "Product.Create";
        public const string ProductUpdate = "Product.Update";
        public const string ProductDelete = "Product.Delete";

        // Category
        public const string CategoryView = "Category.View";
        public const string CategoryCreate = "Category.Create";
        public const string CategoryUpdate = "Category.Update";
        public const string CategoryDelete = "Category.Delete";

        // Review
        public const string ReviewCreate = "Review.Create";
        public const string ReviewDelete = "Review.Delete";

        // Wishlist
        public const string WishlistManage = "Wishlist.Manage";

        // Dashboard
        public const string DashboardView = "Dashboard.View";

        // User
        public const string UserView = "User.View";
        public const string UserDelete = "User.Delete";
        public const string UserLock = "User.Lock";
        public const string UserUnlock = "User.Unlock";

        // Audit
        public const string AuditView = "Audit.View";
    }
}