using AutoMapper;
using MediatR;
using ProductService.Application.Commands.Products.UpdateProduct;
using ProductService.DTOs;
using ProductService.Repositories.UnitOfWork;

namespace ProductService.Application.Commands.Products.UpdateProduct;

public class UpdateProductCommandHandler
    : IRequestHandler<UpdateProductCommand, ProductDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateProductCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductDto?> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id);

        if (product == null)
            return null;

        product.Name = request.Product.Name;
        product.Description = request.Product.Description;
        product.Price = request.Product.Price;
        product.Stock = request.Product.Stock;
        product.ImageUrl = request.Product.ImageUrl;
        product.CategoryId = request.Product.CategoryId;

        await _unitOfWork.Products.UpdateAsync(product);

        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ProductDto>(product);
    }
}