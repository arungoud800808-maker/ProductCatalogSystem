using AutoMapper;
using ProductService.DTOs;
using ProductService.Models;
using ProductService.Repositories;

namespace ProductService.Services;

public class WishlistService : IWishlistService
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public WishlistService(
        IWishlistRepository wishlistRepository,
        IProductRepository productRepository,
        IMapper mapper)
    {
        _wishlistRepository = wishlistRepository;
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task AddAsync(int userId, CreateWishlistDto dto)
    {
        // Check whether the product exists
        var product = await _productRepository.GetByIdAsync(dto.ProductId);

        if (product == null)
        {
            throw new Exception("Product not found.");
        }

        var wishlist = new Wishlist
        {
            UserId = userId,
            ProductId = dto.ProductId
        };

        await _wishlistRepository.AddAsync(wishlist);
    }

    public async Task<IEnumerable<WishlistDto>> GetByUserAsync(int userId)
    {
        var wishlists = await _wishlistRepository.GetByUserAsync(userId);

        return wishlists.Select(w => new WishlistDto
        {
            Id = w.Id,
            ProductId = w.ProductId,
            ProductName = w.Product!.Name,
            Price = w.Product.Price,
            ImageUrl = w.Product.ImageUrl,
            CreatedDate = w.CreatedDate
        });
    }

    public async Task RemoveAsync(int id)
    {
        await _wishlistRepository.RemoveAsync(id);
    }
}