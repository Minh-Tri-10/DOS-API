using System;
using System.Collections.Generic;

namespace MVCApplication.Models;

public partial class Cart
{
    public int CartId { get; set; }

    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual List<CartItem> CartItems { get; set; } = new List<CartItem>();
}
