using MVCApplication.DTOs;
using OrderAPI.DTOs;

namespace MVCApplication.Services.Interfaces
{
    public interface IOrderService
    {
        Task<int?> CreateAsync(CreateOrderDto dto);
        Task<List<OrderDto>> GetAllAsync();
        Task<OrderDto?> GetByIdAsync(int id);
        Task<bool> UpdateAsync(int id, UpdateOrderDto dto);
        Task<List<OrderDto>> GetOrdersByUserIdAsync(int userId); // sửa trả về List<OrderDto>
        Task<bool> CancelAsync(int id, string reason); // hủy đơn
        Task<PagedResult<OrderDto>?> GetPagedAsync(int page = 1, int pageSize = 10);
        Task<bool> MarkAsPaidAsync(int orderId);

        Task<ProductUsageDto> CheckProductUsageAsync(int productId);

    }
}
