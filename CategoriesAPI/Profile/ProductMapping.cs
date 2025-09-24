using AutoMapper;
using CategoriesAPI.DTOs;
using CategoriesAPI.Models;

namespace ProductAPI.Mapping
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            // Map từ Model sang DTO
            CreateMap<Product, ProductDTO>();

            // Map từ DTO sang Model
            CreateMap<CreateProductDTO, Product>();
            CreateMap<UpdateProductDTO, Product>();
        }
    }
}
