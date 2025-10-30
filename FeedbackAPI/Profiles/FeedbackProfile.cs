using AutoMapper;
using FeedbackAPI.Models;
using FeedbackAPI.DTOs;

namespace FeedbackAPI.Profiles
{
    public class FeedbackProfile : Profile
    {
        public FeedbackProfile()
        {
            CreateMap<FeedbackRequestDTO, Feedback>();
            CreateMap<Feedback, FeedbackResponseDTO>();
            CreateMap<FeedbackUpdateDTO, Feedback>()
                // Bỏ qua FeedbackId và OrderId vì chúng không được phép thay đổi
                .ForMember(dest => dest.FeedbackId, opt => opt.Ignore())
                .ForMember(dest => dest.FeedbackDate, opt => opt.Ignore())
                .ForMember(dest => dest.OrderId, opt => opt.Ignore());
        }
    }
}
