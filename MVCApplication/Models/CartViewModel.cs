namespace MVCApplication.Models
{
    public class CartViewModel
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public List<CartItemViewModel> CartItems { get; set; } = new();
    }
}
