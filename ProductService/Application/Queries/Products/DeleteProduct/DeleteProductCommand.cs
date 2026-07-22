using MediatR;

namespace ProductService.Application.Commands.Products.DeleteProduct;

public record DeleteProductCommand(int Id) : IRequest<bool>;