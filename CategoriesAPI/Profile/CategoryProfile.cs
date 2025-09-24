using AutoMapper;
using CategoriesAPI.DTOs;
using CategoriesAPI.Models;

namespace CategoriesAPI.Mapping;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<Category, CategoryDTO>();         

        CreateMap<CreateCategoryDTO, Category>();
        CreateMap<UpdateCategoryDTO, Category>();
    }
}
