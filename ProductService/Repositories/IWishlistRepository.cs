using ProductService.Models;

namespace ProductService.Repositories;

public interface IWishlistRepository
{
    Task AddAsync(Wishlist wishlist);

    Task<IEnumerable<Wishlist>> GetByUserAsync(int userId);

    Task RemoveAsync(int id);

    Task<Wishlist?> GetAsync(int id);
}