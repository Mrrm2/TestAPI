using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TestAPI.Models;

namespace TestAPI.Data;

public class SampleDbContext :IdentityDbContext<IdentityUser>
{
    public SampleDbContext(DbContextOptions<SampleDbContext> options) : base(options) { }

    public DbSet<Order> Orders { get; set; }
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Product>()
            .ToTable("Products")
            .HasData(SampleData.GetProducts());

        modelBuilder.Entity<Order>()
            .ToTable("Orders")
            .HasData(SampleData.GetOrders());

        modelBuilder.Entity<IdentityRole>().HasData(
            new { Id = "1", Name = "TLKAcc", NormalizedName = "TLKACC" }
        );

    }
}