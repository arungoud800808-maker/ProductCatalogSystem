using AutoMapper;
using MediatR;
using ProductService.DTOs;
using ProductService.Repositories.UnitOfWork;
using ProductService.Services.Cache;

namespace ProductService.Application.Queries.Products.GetProductById;

public class GetProductByIdQueryHandler
    : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IRedisCacheService _cache;

    public GetProductByIdQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IRedisCacheService cache)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<ProductDto?> Handle(
    GetProductByIdQuery request,
    CancellationToken cancellationToken)
    {
        var cacheKey = $"Product_{request.Id}";

        // 1. Check Redis
        var cachedProduct =
            await _cache.GetAsync<ProductDto>(cacheKey);

        if (cachedProduct != null)
        {
            Console.WriteLine("Returned from Redis");

            return cachedProduct;
        }

        // 2. Read SQL Server
        var product =
            await _unitOfWork.Products.GetByIdAsync(request.Id);

        if (product == null)
            return null;

        var productDto =
            _mapper.Map<ProductDto>(product);

        // 3. Save into Redis
        await _cache.SetAsync(
            cacheKey,
            productDto,
            TimeSpan.FromMinutes(10));

        Console.WriteLine("Returned from SQL");

        return productDto;
    }
}