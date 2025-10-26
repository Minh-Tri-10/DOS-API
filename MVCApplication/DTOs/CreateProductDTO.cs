using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MVCApplication.DTOs
{
    public class CreateProductDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(100, ErrorMessage = "Tên sản phẩm tối đa 100 ký tự")]
        public string ProductName { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá sản phẩm")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng tồn kho")]
        [Range(0, int.MaxValue, ErrorMessage = "Tồn kho không được âm")]
        public int? Stock { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục sản phẩm")]
        [Range(1, int.MaxValue, ErrorMessage = "Danh mục không hợp lệ")]
        public int? CategoryId { get; set; }

        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ảnh sản phẩm")]
        public IFormFile? ImageFile { get; set; }
    }
}
