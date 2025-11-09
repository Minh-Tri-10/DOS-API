using System.ComponentModel.DataAnnotations;

namespace OrderAPI.DTOs
{
    public class OrderItemDto
    {
        [Key]
        public int OrderItemId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? ProductName { get; set; }    // <-- lấy từ ProductService
        public string? CategoryName { get; set; }   // <-- lấy từ CategoryService
    }
}
