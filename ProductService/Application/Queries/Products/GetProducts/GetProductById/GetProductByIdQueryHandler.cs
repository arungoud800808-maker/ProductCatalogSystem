using AutoMapper;
using MediatR;
using ProductService.DTOs;
using ProductService.Repositories.UnitOfWork;

namespace ProductService.Application.Queries.Products.GetProductById;

public class GetProductByIdQueryHandler
    : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProductByIdQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductDto?> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product =
            await _unitOfWork.Products.GetByIdAsync(request.Id);

        if (product == null)
            return null;

        return _mapper.Map<ProductDto>(product);
    }
}