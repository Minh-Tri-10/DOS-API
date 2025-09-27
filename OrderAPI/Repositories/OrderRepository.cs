using Microsoft.EntityFrameworkCore;
using OrderAPI.Models;
using OrderAPI.Repositories.Interfaces;

namespace OrderAPI.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DrinkOrderDbContext _context;

        public OrderRepository(DrinkOrderDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                //.Include(o => o.User)
                .Include(o => o.OrderItems)
                //.ThenInclude(oi => oi.Product)
                .ToListAsync();
        }

        public async Task<Order> GetByIdAsync(int id) =>
            await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == id);

        public async Task<Order> GetOrderDetailsByIdAsync(int id) =>
            await _context.Orders
                //.Include(o => o.User)
                .Include(o => o.OrderItems)
            //.ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

        public async Task<int> CreateOrderAsync(Order order, List<OrderItem> items)
        {
            /* Hiện tại là test chỉ mới bên api nên có thể không biết orderitem
            có có sẵn trong order khi chuyển từ front end sang hay không, 
            nếu có sẵn rồi thì không cần sửa nữa */

            // Đảm bảo OrderItems rỗng khi Add(order) để tránh EF tự insert
            //order.OrderItems = new List<OrderItem>();
            //_context.Orders.Add(order);
            //await _context.SaveChangesAsync();

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            
            //foreach (var item in items)
            //{
            //    item.OrderId = order.OrderId;
            //    item.OrderItemId = 0; // đảm bảo EF không insert Id (Id tự tăng) // dòng này có thể xem xét xóa đi
            //    _context.OrderItems.Add(item);
            //}
            await _context.SaveChangesAsync();

            return order.OrderId;
        }

        public async Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteOrderAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order != null)
            {
                _context.OrderItems.RemoveRange(order.OrderItems);
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Order>> GetOrdersByUserIdAsync(int userId) =>
            await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ToListAsync();

        //public async Task<User> GetUserByIdAsync(int userId) =>
        //    await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

        public async Task MarkOrderAsPaidAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.PaymentStatus = "Paid";
                await _context.SaveChangesAsync();
            }
        }
    }
}
