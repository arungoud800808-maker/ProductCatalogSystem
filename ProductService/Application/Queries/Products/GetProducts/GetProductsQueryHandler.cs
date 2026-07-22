using AutoMapper;
using MediatR;
using ProductService.DTOs;
using ProductService.Repositories.UnitOfWork;
using ProductService.Specifications;

namespace ProductService.Application.Queries.Products.GetProducts;

public class GetProductsQueryHandler
    : IRequestHandler<GetProductsQuery, IEnumerable<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetProductsQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductDto>> Handle(
     GetProductsQuery request,
     CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.ProductGeneric.GetAllAsync();

        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }
}