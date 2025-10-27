using AutoMapper;
using CategoriesAPI.DTOs;
using CategoriesAPI.Models;

namespace CategoriesAPI.Mapping
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {

            // Map Product -> ProductDTO
            CreateMap<Product, ProductDTO>()
                .ForMember(dest => dest.CategoryName,
                           opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : string.Empty));

            CreateMap<CreateProductDTO, Product>()
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl));
            CreateMap<UpdateProductDTO, Product>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl));
        }
    }
}

