1. Create poject with controllers

```sh
dotnet new webapi --use-controllers -o TestAPI
```

2. Add packages

```sh
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.AspNetCore.Mvc.NewtonsoftJson
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

3. Create Models.Order.cs

```csharp
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
```

4. Create Models.Product.cs

```csharp
public class Product
{
    public int ProductId { get; set; }

    [MaxLength(100)]
    public required string ProductName { get; set; }

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }
}
```

5. Create sample data in Data.SampleData.cs

```csharp
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
```

6. Create Data.SampleDbContext.cs

```csharp
public class SampleDbContext : DbContext
{
    public SampleDbContext(DbContextOptions<SampleDbContext> options) : base(options) { }

    public DbSet<Order> Orders { get; set; }
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .ToTable("Products")
            .HasData(SampleData.GetProducts());

        modelBuilder.Entity<Order>()
            .ToTable("Orders")
            .HasData(SampleData.GetOrders());
    }
}
```

7. Add connection string to appsettings.Development.json

```json
	"ConnectionStrings": {
		"DefaultConnection": "DataSource=Sample.db;Cache=Shared;"
  }
```

8. Add context to Program.cs

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SampleDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddControllers().AddNewtonsoftJson(options =>
  options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
);
```

9. Add migration

```sh
dotnet ef migrations add InitializeDb -o Data/Migrations
dotnet ef database update
```

10. Add controller

```sh
dotnet aspnet-codegenerator controller -name OrdersController -async -api -m Order -dc SampleDbContext -outDir Controllers
dotnet aspnet-codegenerator controller -name ProductsController -async -api -m Product -dc SampleDbContext -outDir Controllers
```
