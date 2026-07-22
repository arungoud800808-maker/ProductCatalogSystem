using MediatR;
using ProductService.DTOs;

namespace ProductService.Application.Queries.Products.GetProductById;

public record GetProductByIdQuery(int Id)
    : IRequest<ProductDto?>;