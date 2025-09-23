using AutoMapper;
using PaymentAPI.DTOs;
using PaymentAPI.Models;

namespace PaymentAPI.Profiles
{
    public class PaymentProfile : Profile
    {
        public PaymentProfile()
        {
            CreateMap<PaymentRequestDTO, Payment>();
            CreateMap<PaymentUpdateDTO, Payment>();
            CreateMap<Payment, PaymentResultDTO>();
            CreateMap<Payment, PaymentResponseDTO>();
        }
    }
}
