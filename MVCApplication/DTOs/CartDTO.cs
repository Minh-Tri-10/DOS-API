namespace MVCApplication.DTOs
{
    public class CartDTO
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public List<CartItemDTO> CartItems { get; set; } = new();
    }

}
