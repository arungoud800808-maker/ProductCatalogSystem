using MediatR;
using ProductService.DTOs;

namespace ProductService.Application.Commands.Products.CreateProduct;

public record CreateProductCommand(
    CreateProductDto Product
) : IRequest<ProductDto>;