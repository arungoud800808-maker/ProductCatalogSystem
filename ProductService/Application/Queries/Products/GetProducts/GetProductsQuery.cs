using MediatR;
using ProductService.DTOs;

namespace ProductService.Application.Queries.Products.GetProducts;

public record GetProductsQuery(
    string? Search = null,
    string? Sort = null,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<IEnumerable<ProductDto>>;