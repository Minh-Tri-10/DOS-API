using System;
using System.Collections.Generic;

namespace CategoriesAPI.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int? Stock { get; set; }

    public int? CategoryId { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Category? Category { get; set; }  // Thêm dòng này để sửa lỗi

}
