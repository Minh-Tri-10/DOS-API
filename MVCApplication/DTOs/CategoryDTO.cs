using System.ComponentModel.DataAnnotations;

namespace MVCApplication.DTOs
{
    public class CategoryDTO
    {
        public int CategoryId { get; set; }

        [Display(Name = "Tên danh mục")]
        public string CategoryName { get; set; } = null!;

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        public List<int> ProductIds { get; set; } = new List<int>(); // Chỉ Id để tránh tải full Products
    }

    public class CreateCategoryDTO
    {
        [Display(Name = "Tên danh mục")]
        [Required(ErrorMessage = "Tên danh mục là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự.")]
        public string CategoryName { get; set; } = null!;

        [Display(Name = "Mô tả")]
        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string? Description { get; set; }
    }

    public class UpdateCategoryDTO
    {
        [Display(Name = "Tên danh mục")]
        [Required(ErrorMessage = "Tên danh mục là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự.")]
        public string CategoryName { get; set; } = null!;

        [Display(Name = "Mô tả")]
        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự.")]
        public string? Description { get; set; }
    }
}