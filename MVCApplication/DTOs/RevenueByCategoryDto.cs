namespace MVCApplication.DTOs
{
    public sealed class RevenueByCategoryDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public decimal Revenue { get; set; }
    }

}
