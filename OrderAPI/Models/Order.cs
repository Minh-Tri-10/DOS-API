using System;
using System.Collections.Generic;

namespace OrderAPI.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public string? OrderStatus { get; set; }

    public DateTime OrderDate { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? CancelReason { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    //public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    //public virtual User User { get; set; } = null!;
}
