using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestAPI.Models;

public class Order
{
    public int Id { get; set; }

    [ForeignKey("ProductId")]
    public int ProductId { get; set; }

    public int Quantity { get; set; }

    [MaxLength(20)]
    public required string Status { get; set; }

    public DateTime OrderDate { get; set; }
}