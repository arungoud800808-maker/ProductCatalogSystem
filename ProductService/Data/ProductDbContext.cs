using Microsoft.EntityFrameworkCore;
using ProductService.Models;

namespace ProductService.Data
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products => Set<Product>();

        public DbSet<User> Users => Set<User>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Review> Reviews => Set<Review>();

        public DbSet<Wishlist> Wishlists => Set<Wishlist>();

        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
    .HasQueryFilter(p => !p.IsDeleted);

            modelBuilder.Entity<Category>()
                .HasQueryFilter(c => !c.IsDeleted);

            modelBuilder.Entity<Review>()
                .HasQueryFilter(r => !r.IsDeleted);
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Review>()
    .HasOne(r => r.Product)
    .WithMany(p => p.Reviews)
    .HasForeignKey(r => r.ProductId);

            modelBuilder.Entity<Wishlist>()
    .HasOne(w => w.User)
    .WithMany(u => u.Wishlists)
    .HasForeignKey(w => w.UserId);

            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.Product)
                .WithMany(p => p.Wishlists)
                .HasForeignKey(w => w.ProductId);

            modelBuilder.Entity<Wishlist>()
                .HasIndex(w => new { w.UserId, w.ProductId })
                .IsUnique();
        }
    }
}