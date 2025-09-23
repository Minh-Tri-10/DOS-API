using AutoMapper;
using CategoriesAPI.DTOs;
using CategoriesAPI.Models;

namespace CategoriesAPI.Mapping;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<Category, CategoryDTO>()
            .ForMember(dest => dest.ProductIds, opt => opt.MapFrom(src => src.Products.Select(p => p.ProductId)));

        CreateMap<CreateCategoryDTO, Category>();
        CreateMap<UpdateCategoryDTO, Category>();
    }
}
