using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace FeedbackAPI.Models;

public partial class DosfeedbackDbContext : DbContext
{
    public DosfeedbackDbContext()
    {
    }

    public DosfeedbackDbContext(DbContextOptions<DosfeedbackDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    { }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__6A4BEDF6A2A538F6");

            entity.HasIndex(e => e.OrderId, "UQ_OrderFeedback").IsUnique();

            entity.Property(e => e.FeedbackId).HasColumnName("FeedbackID");
            entity.Property(e => e.FeedbackDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OrderId).HasColumnName("OrderID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
