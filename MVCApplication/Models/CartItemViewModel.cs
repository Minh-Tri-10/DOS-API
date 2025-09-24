namespace MVCApplication.Models
{
    public class CartItemViewModel
    {
        public int CartItemId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        // Dữ liệu bổ sung từ ProductAPI
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
    }
}
