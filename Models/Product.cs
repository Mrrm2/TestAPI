using System;
using System.ComponentModel.DataAnnotations;

namespace TestAPI.Models;

public class Product
{
    public int ProductId { get; set; }

    [MaxLength(100)]
    public required string ProductName { get; set; }

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }
}