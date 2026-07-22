using MediatR;
using ProductService.DTOs;

namespace ProductService.Application.Commands.Products.UpdateProduct
{ 

    public record UpdateProductCommand(
      int Id,
      UpdateProductDto Product
  ) : IRequest<ProductDto?>;
}
