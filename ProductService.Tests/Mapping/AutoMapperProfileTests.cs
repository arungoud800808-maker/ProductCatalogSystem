using AutoMapper;
using ProductService.DTOs;
using ProductService.Models;

namespace ProductService.Mapping;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Product -> ProductDto
        CreateMap<Product, ProductDto>()
            .ForMember(
                dest => dest.CategoryName,
                opt => opt.MapFrom(src =>
                    src.Category != null
                        ? src.Category.Name
                        : null));


        // Product -> ProductV2Dto
        CreateMap<Product, ProductV2Dto>()
            .ForMember(
                dest => dest.CategoryName,
                opt => opt.MapFrom(src =>
                    src.Category != null
                        ? src.Category.Name
                        : null));


        // Create Product DTO -> Product
        CreateMap<CreateProductDto, Product>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.CreatedDate, opt => opt.Ignore())
            .ForMember(x => x.UpdatedDate, opt => opt.Ignore())
            .ForMember(x => x.Category, opt => opt.Ignore())
            .ForMember(x => x.Reviews, opt => opt.Ignore())
            .ForMember(x => x.Wishlists, opt => opt.Ignore())
            .ForMember(x => x.IsDeleted, opt => opt.Ignore())
            .ForMember(x => x.DeletedAt, opt => opt.Ignore());


        // Update Product DTO -> Product
        CreateMap<UpdateProductDto, Product>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.CreatedDate, opt => opt.Ignore())
            .ForMember(x => x.UpdatedDate, opt => opt.Ignore())
            .ForMember(x => x.Category, opt => opt.Ignore())
            .ForMember(x => x.Reviews, opt => opt.Ignore())
            .ForMember(x => x.Wishlists, opt => opt.Ignore())
            .ForMember(x => x.IsDeleted, opt => opt.Ignore())
            .ForMember(x => x.DeletedAt, opt => opt.Ignore());


        // Category mappings
        CreateMap<Category, CategoryDto>();

        CreateMap<CreateCategoryDto, Category>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.Products, opt => opt.Ignore())
            .ForMember(x => x.IsDeleted, opt => opt.Ignore())
            .ForMember(x => x.DeletedAt, opt => opt.Ignore());


        CreateMap<UpdateCategoryDto, Category>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.Products, opt => opt.Ignore())
            .ForMember(x => x.IsDeleted, opt => opt.Ignore())
            .ForMember(x => x.DeletedAt, opt => opt.Ignore());


        // Review mappings
        CreateMap<Review, ReviewDto>();

        CreateMap<CreateReviewDto, Review>()
            .ForMember(x => x.Id, opt => opt.Ignore())
            .ForMember(x => x.Product, opt => opt.Ignore())
            .ForMember(x => x.CreatedDate, opt => opt.Ignore())
            .ForMember(x => x.IsDeleted, opt => opt.Ignore())
            .ForMember(x => x.DeletedAt, opt => opt.Ignore());


        // Deleted Product
        CreateMap<Product, DeletedProductDto>();
    }
}