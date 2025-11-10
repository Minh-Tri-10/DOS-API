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
        //Task<PagedResult<OrderDto>?> GetPagedAsync(int page = 1, int pageSize = 10);
        Task<bool> MarkAsPaidAsync(int orderId);

        Task<ProductUsageDto> CheckProductUsageAsync(int productId);
        /// <summary>
        /// Odata
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="search"></param>
        /// <param name="status"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        Task<PagedResult<OrderDto>?> GetPagedAsync(int page = 1, int pageSize = 10, string? search = null, string? status = null, string? payment = null);
        Task<bool> MarkOrderAsCompletedAsync(int orderId);
    }
}
