using OrderAPI.DTOs;
using OrderAPI.Models;
using AutoMapper;

namespace OrderAPI.Profiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            // Entity -> DTO
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems));

            CreateMap<OrderItem, OrderItemDto>();

            // DTO -> Entity
            CreateMap<CreateOrderDto, Order>()
                .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.Items))
                .ForMember(dest => dest.TotalAmount,
                           opt => opt.MapFrom(src => src.Items.Sum(i => i.Quantity * i.UnitPrice)))
                .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(_ => "Pending"))
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(_ => "Unpaid"));

            CreateMap<CreateOrderItemDto, OrderItem>();

            CreateMap<UpdateOrderDto, Order>()
            //bỏ qua những property null trong UpdateOrderDto, tránh ghi đè giá trị cũ trong Order
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
