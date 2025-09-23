namespace CategoriesAPI.DTOs
{
    public class CategoryDTO
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<int> ProductIds { get; set; } = new List<int>(); // Chỉ Id để tránh tải full Products
    }

    public class CreateCategoryDTO
    {
        public string CategoryName { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class UpdateCategoryDTO
    {
        public string CategoryName { get; set; } = null!;
        public string? Description { get; set; }
    }
}
