using OrderAPI.DTOs;

namespace OrderAPI.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetAllAsync();
        Task<OrderDto> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateOrderDto dto);
        Task UpdateAsync(int id, UpdateOrderDto dto);
        Task DeleteAsync(int id);
        Task<List<OrderDto>> GetOrdersByUserIdAsync(int userId);
        Task MarkAsPaidAsync(int orderId);
        // Hủy đơn hàng
        Task<bool> CancelAsync(int orderId, string cancelReason);

    }
}
