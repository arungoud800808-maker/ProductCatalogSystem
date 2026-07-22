using ProductService.DTOs;

namespace ProductService.Services;

public interface IWishlistService
{
    Task AddAsync(int userId, CreateWishlistDto dto);

    Task<IEnumerable<WishlistDto>> GetByUserAsync(int userId);

    Task RemoveAsync(int id);
}