using System;
using TestAPI.Models;

namespace TestAPI.Data;

public static class SampleData
{
    public static List<Product> GetProducts()
    {
        return new List<Product>()
        {
            new Product()
            {
                ProductId = 1,
                ProductName = "Laptop",
                Price = 999.99m,
                StockQuantity = 50
            },
            new Product()
            {
                ProductId = 2,
                ProductName = "Smartphone",
                Price = 499.99m,
                StockQuantity = 100
            },
            new Product()
            {
                ProductId = 3,
                ProductName = "Headphones",
                Price = 199.99m,
                StockQuantity = 75
            },
            new Product()
            {
                ProductId = 4,
                ProductName = "Smartwatch",
                Price = 249.99m,
                StockQuantity = 30
            },
            new Product()
            {
                ProductId = 5,
                ProductName = "Gaming Console",
                Price = 399.99m,
                StockQuantity = 20
            },
            new Product()
            {
                ProductId = 6,
                ProductName = "Wireless Mouse",
                Price = 49.99m,
                StockQuantity = 150
            },
            new Product()
            {
                ProductId = 7,
                ProductName = "Bluetooth Speaker",
                Price = 99.99m,
                StockQuantity = 80
            },
            new Product()
            {
                ProductId = 8,
                ProductName = "External Hard Drive",
                Price = 79.99m,
                StockQuantity = 60
            },
            new Product()
            {
                ProductId = 9,
                ProductName = "USB-C Hub",
                Price = 29.99m,
                StockQuantity = 200
            },
            new Product()
            {
                ProductId = 10,
                ProductName = "Monitor",
                Price = 299.99m,
                StockQuantity = 40
            },
        };
    }

    public static List<Order> GetOrders()
    {
        return new List<Order>()
        {
            new Order()
            {
                Id = 1,
                ProductId = 1,
                Quantity = 1,
                Status = "Completed",
                OrderDate = new DateTime(2024, 9, 25)
            },
            new Order()
            {
                Id = 2,
                ProductId = 2,
                Quantity = 2,
                Status = "Pending",
                OrderDate = new DateTime(2024, 9, 26)
            },
            new Order()
            {
                Id = 3,
                ProductId = 3,
                Quantity = 5,
                Status = "Shipped",
                OrderDate = new DateTime(2024, 9, 27)
            },
            new Order()
            {
                Id = 4,
                ProductId = 4,
                Quantity = 1,
                Status = "Completed",
                OrderDate = new DateTime(2024, 9, 28)
            },
            new Order()
            {
                Id = 5,
                ProductId = 5,
                Quantity = 1,
                Status = "Pending",
                OrderDate = new DateTime(2024, 9, 29)
            },
            new Order()
            {
                Id = 6,
                ProductId = 6,
                Quantity = 3,
                Status = "Shipped",
                OrderDate = new DateTime(2024, 9, 30)
            },
            new Order()
            {
                Id = 7,
                ProductId = 7,
                Quantity = 2,
                Status = "Completed",
                OrderDate = new DateTime(2024, 9, 30)
            },
            new Order()
            {
                Id = 8,
                ProductId = 8,
                Quantity = 1,
                Status = "Pending",
                OrderDate = new DateTime(2024, 9, 30)
            },
            new Order()
            {
                Id = 9,
                ProductId = 9,
                Quantity = 4,
                Status = "Shipped",
                OrderDate = new DateTime(2024, 9, 30)

            },
            new Order()
            {
                Id = 10,
                ProductId = 10,
                Quantity = 2,
                Status = "Completed",
                OrderDate = new DateTime(2024, 9, 30)
            },
        };
    }
}