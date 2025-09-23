using OrderAPI.Models;

namespace OrderAPI.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order> GetByIdAsync(int id);
        Task<Order> GetOrderDetailsByIdAsync(int id);
        Task<int> CreateOrderAsync(Order order, List<OrderItem> items);
        Task UpdateAsync(Order order);
        Task DeleteOrderAsync(int orderId);
        Task<List<Order>> GetOrdersByUserIdAsync(int userId);
        Task<User> GetUserByIdAsync(int userId);
        Task MarkOrderAsPaidAsync(int orderId);
    }
}
