using Microsoft.EntityFrameworkCore;
using OrderAPI.Models;
using OrderAPI.Repositories.Interfaces;

namespace OrderAPI.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _context;

        public OrderRepository(OrderDbContext context)
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
        public async Task<(List<Order>, int)> GetPagedAsync(int page, int pageSize)
        {
            var query = _context.Orders
                //.Include(o => o.User)
                .OrderByDescending(o => o.OrderDate);

            var totalCount = await query.CountAsync();

            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new Order
                {
                    OrderId = o.OrderId,
                    UserId = o.UserId,
                    //FullName = o.User.FullName,
                    OrderStatus = o.OrderStatus,
                    PaymentStatus = o.PaymentStatus,
                    TotalAmount = o.TotalAmount,
                    OrderDate = o.OrderDate
                })
                .ToListAsync();

            return (orders, totalCount);
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
            /* Hi?n t?i là test ch? m?i bên api nên có th? không bi?t orderitem
            có có s?n trong order khi chuy?n t? front end sang hay không, 
            n?u có s?n r?i th́ không c?n s?a n?a */

            // Đ?m b?o OrderItems r?ng khi Add(order) d? tránh EF t? insert
            //order.OrderItems = new List<OrderItem>();
            //_context.Orders.Add(order);
            //await _context.SaveChangesAsync();

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            
            //foreach (var item in items)
            //{
            //    item.OrderId = order.OrderId;
            //    item.OrderItemId = 0; // d?m b?o EF không insert Id (Id t? tang) // ḍng này có th? xem xét xóa di
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

        public async Task<int> CountOrdersContainingProductAsync(int productId)
        {
            return await _context.OrderItems
                .Where(oi => oi.ProductId == productId)
                .Select(oi => oi.OrderId)
                .Distinct()  // Đếm số Order unique
                .CountAsync();
        }

    }
}
