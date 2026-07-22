using AutoMapper;
using ProductService.DTOs;
using ProductService.Models;

namespace ProductService.Mapping;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Product, ProductDto>()
    .ForMember(
        dest => dest.CategoryName,
        opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));

        CreateMap<Product, ProductV2Dto>()
            .ForMember(
                d => d.CategoryName,
                o => o.MapFrom(s =>
                    s.Category != null
                        ? s.Category.Name
                        : null));

        CreateMap<CreateProductDto, Product>();

        CreateMap<UpdateProductDto, Product>();

        CreateMap<Category, CategoryDto>();

        CreateMap<CreateCategoryDto, Category>();

        CreateMap<UpdateCategoryDto, Category>();
        CreateMap<Review, ReviewDto>();

        CreateMap<CreateReviewDto, Review>();
        CreateMap<Product, DeletedProductDto>();
    }
}