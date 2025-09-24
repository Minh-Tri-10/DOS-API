using AutoMapper;
using CartAPI.Models;
using CartAPI.DTOs;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Cart -> CartDTO
        CreateMap<Cart, CartDTO>();

        // CartItem -> CartItemDTO
        CreateMap<CartItem, CartItemDTO>();
    }
}
