namespace MVCApplication.Models
{
    public class CartItemViewModel
    {
        public int CartItemId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        // Product metadata resolved via CategoriesAPI
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; } // số lượng tồn kho trong Product
    }
}


