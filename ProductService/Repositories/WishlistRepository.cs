using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;

namespace ProductService.Repositories;

public class WishlistRepository : IWishlistRepository
{
    private readonly ProductDbContext _context;

    public WishlistRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Wishlist wishlist)
    {
        _context.Wishlists.Add(wishlist);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Wishlist>> GetByUserAsync(int userId)
    {
        return await _context.Wishlists
            .Include(w => w.Product)
            .Where(w => w.UserId == userId)
            .ToListAsync();
    }

    public async Task<Wishlist?> GetAsync(int id)
    {
        return await _context.Wishlists.FindAsync(id);
    }

    public async Task RemoveAsync(int id)
    {
        var wishlist = await _context.Wishlists.FindAsync(id);

        if (wishlist == null)
            return;

        _context.Wishlists.Remove(wishlist);

        await _context.SaveChangesAsync();
    }
}