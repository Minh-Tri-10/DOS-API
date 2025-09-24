using System.ComponentModel.DataAnnotations;

namespace CategoriesAPI.DTOs
{
    public class UpdateProductDTO
    {
        [Required] public string ProductName { get; set; } = null!;
        public string? Description { get; set; }
        [Range(0.01, double.MaxValue)] public decimal Price { get; set; }
        [Range(0, int.MaxValue)] public int? Stock { get; set; }
        public int? CategoryId { get; set; }
        public string? ImageUrl { get; set; }
    }
}
