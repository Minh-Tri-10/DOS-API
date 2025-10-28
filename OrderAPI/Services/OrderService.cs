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
        private readonly IUserClient _userClient;
        private readonly ICatalogProductClient _catalogProductClient;
        private readonly ICategoryClient _categoryClient;
        public OrderService(IOrderRepository repo, IMapper mapper, IUserClient userClient, ICatalogProductClient productClient
            , ICategoryClient categoryClient)
        {
            _repo = repo;
            _mapper = mapper;
            _userClient = userClient;
            _catalogProductClient = productClient;
            _categoryClient = categoryClient;
        }

        //public async Task<IEnumerable<OrderDto>> GetAllAsync()
        //{
        //    var orders = await _repo.GetAllAsync();
        //    return _mapper.Map<IEnumerable<OrderDto>>(orders);
        //}

        //public async Task<OrderDto> GetByIdAsync(int id)
        //{
        //    var order = await _repo.GetOrderDetailsByIdAsync(id);
        //    return _mapper.Map<OrderDto>(order);
        //}
        public async Task<IEnumerable<OrderDto>> GetAllAsync()
        {
            var orders = await _repo.GetAllAsync();
            var orderDtos = _mapper.Map<IEnumerable<OrderDto>>(orders);

            foreach (var dto in orderDtos)
            {
                dto.FullName = await _userClient.GetFullNameByIdAsync(dto.UserId);

                // Enrich with product and category data from CategoriesAPI
                foreach (var item in dto.Items)
                {
                    var product = await _catalogProductClient.GetProductByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        item.ProductName = product.ProductName;
                        if (product.CategoryId > 0)
                        {
                            var category = await _categoryClient.GetCategoryByIdAsync(product.CategoryId);
                            item.CategoryName = category?.CategoryName;
                        }
                    }
                }
            }

            return orderDtos;
        }
        public async Task<(List<OrderDto>, int)> GetPagedAsync(int page, int pageSize)
        {
            var (orders, totalCount) = await _repo.GetPagedAsync(page, pageSize);

            // Map to DTO
            var orderDtos = _mapper.Map<List<OrderDto>>(orders);

            // Enrich data by calling supporting services
            foreach (var dto in orderDtos)
            {
                // Fetch user name
                dto.FullName = await _userClient.GetFullNameByIdAsync(dto.UserId);

                // Fetch product and category details
                foreach (var item in dto.Items)
                {
                    var product = await _catalogProductClient.GetProductByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        item.ProductName = product.ProductName;

                        if (product.CategoryId > 0)
                        {
                            var category = await _categoryClient.GetCategoryByIdAsync(product.CategoryId);
                            item.CategoryName = category?.CategoryName;
                        }
                    }
                }
            }

            return (orderDtos, totalCount);
        }
        public async Task<OrderDto> GetByIdAsync(int id)
        {
            var order = await _repo.GetOrderDetailsByIdAsync(id);
            var dto = _mapper.Map<OrderDto>(order);

            dto.FullName = await _userClient.GetFullNameByIdAsync(dto.UserId);

            foreach (var item in dto.Items)
            {
                var product = await _catalogProductClient.GetProductByIdAsync(item.ProductId);
                if (product != null)
                {
                    item.ProductName = product.ProductName;
                    if (product.CategoryId > 0)
                    {
                        var category = await _categoryClient.GetCategoryByIdAsync(product.CategoryId);
                        item.CategoryName = category?.CategoryName;
                    }
                }
            }

            return dto;
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
            var orderDtos = _mapper.Map<List<OrderDto>>(orders);

            // Fetch user full name from Account service
            var fullName = await _userClient.GetFullNameByIdAsync(userId);

            // Apply full name to each order
            foreach (var dto in orderDtos)
            {
                dto.FullName = fullName;
            }
            return orderDtos;
        }

        public async Task MarkAsPaidAsync(int orderId) =>
            await _repo.MarkOrderAsPaidAsync(orderId);

        // Hủy đơn hàng
        public async Task<bool> CancelAsync(int orderId, string cancelReason)
        {
            var order = await _repo.GetByIdAsync(orderId);
            if (order == null || order.OrderStatus == "cancelled")
                return false;

            order.OrderStatus = "cancelled";
            order.CancelReason = cancelReason;
            await _repo.UpdateAsync(order);

            return true;
        }

    }
}

