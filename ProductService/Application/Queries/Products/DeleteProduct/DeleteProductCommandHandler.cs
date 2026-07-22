using MediatR;
using ProductService.Repositories.UnitOfWork;

namespace ProductService.Application.Commands.Products.DeleteProduct;

public class DeleteProductCommandHandler
    : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(
        DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id);

        if (product == null)
            return false;

        await _unitOfWork.Products.DeleteAsync(request.Id);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}