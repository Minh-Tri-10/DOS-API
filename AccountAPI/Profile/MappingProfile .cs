// Mapping/MappingProfile.cs
using AutoMapper;
using AccountAPI.Models;
using AccountAPI.DTOs;

namespace AccountAPI.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User -> UserDTO
            CreateMap<User, UserDTO>()
                .ForMember(d => d.IsBanned, o => o.MapFrom(s => s.IsBanned ?? false));

            // RegisterDTO -> User (bỏ qua PasswordHash, đặt mặc định khác ở service)
            CreateMap<RegisterDTO, User>()
                .ForMember(d => d.PasswordHash, o => o.Ignore())
                .ForMember(d => d.Role, o => o.MapFrom(_ => "customer"))
                .ForMember(d => d.CreatedAt, o => o.MapFrom(_ => DateTime.UtcNow));

            // UpdateProfileDTO -> User (chỉ map các field khác null)
            CreateMap<UpdateProfileDTO, User>()
                .ForAllMembers(o => o.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
