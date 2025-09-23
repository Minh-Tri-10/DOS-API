using MVCApplication.DTOs;

namespace MVCApplication.Services.Interfaces
{
    public interface IOrderService
    {
        Task<int?> CreateAsync(CreateOrderDto dto);
        Task<List<OrderDto>> GetAllAsync();
        Task<OrderDto?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(int id, UpdateOrderDto dto);
    }
}
