using AutoMapper;
using CategoriesAPI.DTOs;
using CategoriesAPI.Models;

namespace ProductAPI.Mapping
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {

            // Map Product -> ProductDTO
            CreateMap<Product, ProductDTO>()
                .ForMember(dest => dest.CategoryName,
                           opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : string.Empty));

            // Map từ DTO sang Model
            CreateMap<CreateProductDTO, Product>();
            CreateMap<UpdateProductDTO, Product>();
        }
    }
}
