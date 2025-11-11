using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace OrderAPI.Models;

public partial class OrderDbContext : DbContext
{
    public OrderDbContext()
    {
    }

    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    //public virtual DbSet<Cart> Carts { get; set; }

    //public virtual DbSet<CartItem> CartItems { get; set; }

    //public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    //public virtual DbSet<Payment> Payments { get; set; }

    //public virtual DbSet<Product> Products { get; set; }

    //public virtual DbSet<User> Users { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=ADMIN-PC;Database=DrinkOrderDB;User ID=sa;Password=admin@123;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder.Entity<Cart>(entity =>
        //{
        //    entity.HasKey(e => e.CartId).HasName("PK__Carts__51BCD7973185D1D4");

        //    entity.Property(e => e.CartId).HasColumnName("CartID");
        //    entity.Property(e => e.CreatedAt)
        //        .HasDefaultValueSql("(getdate())")
        //        .HasColumnType("datetime");
        //    entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        //    entity.Property(e => e.UserId).HasColumnName("UserID");

        //    entity.HasOne(d => d.User).WithMany(p => p.Carts)
        //        .HasForeignKey(d => d.UserId)
        //        .OnDelete(DeleteBehavior.ClientSetNull)
        //        .HasConstraintName("FK__Carts__UserID__4222D4EF");
        //});

        //modelBuilder.Entity<CartItem>(entity =>
        //{
        //    entity.HasKey(e => e.CartItemId).HasName("PK__CartItem__488B0B2AA7B2AF9A");

        //    entity.Property(e => e.CartItemId).HasColumnName("CartItemID");
        //    entity.Property(e => e.CartId).HasColumnName("CartID");
        //    entity.Property(e => e.CreatedAt)
        //        .HasDefaultValueSql("(getdate())")
        //        .HasColumnType("datetime");
        //    entity.Property(e => e.ProductId).HasColumnName("ProductID");
        //    entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

        //    entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
        //        .HasForeignKey(d => d.CartId)
        //        .OnDelete(DeleteBehavior.ClientSetNull)
        //        .HasConstraintName("FK__CartItems__CartI__45F365D3");

        //    entity.HasOne(d => d.Product).WithMany(p => p.CartItems)
        //        .HasForeignKey(d => d.ProductId)
        //        .OnDelete(DeleteBehavior.ClientSetNull)
        //        .HasConstraintName("FK__CartItems__Produ__46E78A0C");
        //});

        //modelBuilder.Entity<Category>(entity =>
        //{
        //    entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A2B5855EDCB");

        //    entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
        //    entity.Property(e => e.CategoryName).HasMaxLength(100);
        //    entity.Property(e => e.CreatedAt)
        //        .HasDefaultValueSql("(getdate())")
        //        .HasColumnType("datetime");
        //    entity.Property(e => e.Description).HasMaxLength(255);
        //    entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        //});

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BAF5FEE1DDC");

            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.CancelReason).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OrderStatus).HasMaxLength(20);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            //entity.HasOne(d => d.User).WithMany(p => p.Orders)
            //    .HasForeignKey(d => d.UserId)
            //    .OnDelete(DeleteBehavior.ClientSetNull)
            //    .HasConstraintName("FK__Orders__UserID__4AB81AF0");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("PK__OrderIte__57ED06A1250DF4C0");

            entity.Property(e => e.OrderItemId).HasColumnName("OrderItemID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderItem__Order__4F7CD00D");

            //entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
            //    .HasForeignKey(d => d.ProductId)
            //    .OnDelete(DeleteBehavior.ClientSetNull)
            //    .HasConstraintName("FK__OrderItem__Produ__5070F446");
        });

        //modelBuilder.Entity<Payment>(entity =>
        //{
        //    entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A586BA93401");

        //    entity.Property(e => e.PaymentId).HasColumnName("PaymentID");
        //    entity.Property(e => e.CreatedAt)
        //        .HasDefaultValueSql("(getdate())")
        //        .HasColumnType("datetime");
        //    entity.Property(e => e.OrderId).HasColumnName("OrderID");
        //    entity.Property(e => e.PaidAmount).HasColumnType("decimal(10, 2)");
        //    entity.Property(e => e.PaymentDate).HasColumnType("datetime");
        //    entity.Property(e => e.PaymentMethod).HasMaxLength(50);
        //    entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

        //    entity.HasOne(d => d.Order).WithMany(p => p.Payments)
        //        .HasForeignKey(d => d.OrderId)
        //        .OnDelete(DeleteBehavior.ClientSetNull)
        //        .HasConstraintName("FK__Payments__OrderI__5441852A");
        //});

        //modelBuilder.Entity<Product>(entity =>
        //{
        //    entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6ED01A60882");

        //    entity.Property(e => e.ProductId).HasColumnName("ProductID");
        //    entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
        //    entity.Property(e => e.CreatedAt)
        //        .HasDefaultValueSql("(getdate())")
        //        .HasColumnType("datetime");
        //    entity.Property(e => e.Description).HasMaxLength(255);
        //    entity.Property(e => e.ImageUrl).HasMaxLength(255);
        //    entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
        //    entity.Property(e => e.ProductName).HasMaxLength(100);
        //    entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

        //    entity.HasOne(d => d.Category).WithMany(p => p.Products)
        //        .HasForeignKey(d => d.CategoryId)
        //        .HasConstraintName("FK__Products__Catego__3E52440B");
        //});

        //modelBuilder.Entity<User>(entity =>
        //{
        //    entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC51D75BBA");

        //    entity.Property(e => e.UserId).HasColumnName("UserID");
        //    entity.Property(e => e.AvatarUrl).HasMaxLength(255);
        //    entity.Property(e => e.CreatedAt)
        //        .HasDefaultValueSql("(getdate())")
        //        .HasColumnType("datetime");
        //    entity.Property(e => e.Email).HasMaxLength(100);
        //    entity.Property(e => e.FullName).HasMaxLength(100);
        //    entity.Property(e => e.IsBanned).HasDefaultValue(false);
        //    entity.Property(e => e.PasswordHash).HasMaxLength(255);
        //    entity.Property(e => e.Phone).HasMaxLength(20);
        //    entity.Property(e => e.Role).HasMaxLength(20);
        //    entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        //    entity.Property(e => e.Username).HasMaxLength(50);
        //});

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
