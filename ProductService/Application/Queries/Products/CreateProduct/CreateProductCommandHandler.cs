using AutoMapper;
using MediatR;
using ProductService.DTOs;
using ProductService.Models;
using ProductService.Repositories.UnitOfWork;

namespace ProductService.Application.Commands.Products.CreateProduct;

public class CreateProductCommandHandler
    : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateProductCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = _mapper.Map<Product>(request.Product);

        await _unitOfWork.Products.AddAsync(product);

        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ProductDto>(product);
    }
}