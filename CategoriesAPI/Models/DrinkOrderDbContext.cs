using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CategoriesAPI.Models;

public partial class DrinkOrderDbContext : DbContext
{
    public DrinkOrderDbContext()
    {
    }

    public DrinkOrderDbContext(DbContextOptions<DrinkOrderDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<Product> Products { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A2BACEBD097");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6EDB70D3289");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CreatedAt)
            .HasDefaultValueSql("(getdate())")
            .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ProductName).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.HasOne(d => d.Category).WithMany(p => p.Products)
            .HasForeignKey(d => d.CategoryId)
            .HasConstraintName("FK__Products__Catego__778AC167");
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}