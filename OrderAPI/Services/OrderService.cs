using AutoMapper;
using OrderAPI.DTOs;
using OrderAPI.Models;
using OrderAPI.Repositories.Interfaces;
using OrderAPI.Services.Interfaces;

namespace OrderAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repo;
        private readonly IMapper _mapper;

        public OrderService(IOrderRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<OrderDto>> GetAllAsync()
        {
            var orders = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<OrderDto> GetByIdAsync(int id)
        {
            var order = await _repo.GetOrderDetailsByIdAsync(id);
            return _mapper.Map<OrderDto>(order);
        }

        public async Task<int> CreateAsync(CreateOrderDto dto)
        {
            var order = _mapper.Map<Order>(dto);
            return await _repo.CreateOrderAsync(order, order.OrderItems.ToList());
        }

        public async Task UpdateAsync(int id, UpdateOrderDto dto)
        {
            var order = await _repo.GetByIdAsync(id);
            if (order == null) throw new Exception("Order not found");

            _mapper.Map(dto, order);
            await _repo.UpdateAsync(order);
        }

        public async Task DeleteAsync(int id) =>
            await _repo.DeleteOrderAsync(id);

        public async Task<List<OrderDto>> GetOrdersByUserIdAsync(int userId)
        {
            var orders = await _repo.GetOrdersByUserIdAsync(userId);
            return _mapper.Map<List<OrderDto>>(orders);
        }

        public async Task MarkAsPaidAsync(int orderId) =>
            await _repo.MarkOrderAsPaidAsync(orderId);
    }
}
