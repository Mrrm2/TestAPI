using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TestAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitializeDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    OrderDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "TEXT", nullable: false),
                    StockQuantity = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "OrderDate", "ProductId", "Quantity", "Status" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 9, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 1, "Completed" },
                    { 2, new DateTime(2024, 9, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, 2, "Pending" },
                    { 3, new DateTime(2024, 9, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, 5, "Shipped" },
                    { 4, new DateTime(2024, 9, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, 1, "Completed" },
                    { 5, new DateTime(2024, 9, 29, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, 1, "Pending" },
                    { 6, new DateTime(2024, 9, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), 6, 3, "Shipped" },
                    { 7, new DateTime(2024, 9, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), 7, 2, "Completed" },
                    { 8, new DateTime(2024, 9, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), 8, 1, "Pending" },
                    { 9, new DateTime(2024, 9, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), 9, 4, "Shipped" },
                    { 10, new DateTime(2024, 9, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), 10, 2, "Completed" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductId", "Price", "ProductName", "StockQuantity" },
                values: new object[,]
                {
                    { 1, 999.99m, "Laptop", 50 },
                    { 2, 499.99m, "Smartphone", 100 },
                    { 3, 199.99m, "Headphones", 75 },
                    { 4, 249.99m, "Smartwatch", 30 },
                    { 5, 399.99m, "Gaming Console", 20 },
                    { 6, 49.99m, "Wireless Mouse", 150 },
                    { 7, 99.99m, "Bluetooth Speaker", 80 },
                    { 8, 79.99m, "External Hard Drive", 60 },
                    { 9, 29.99m, "USB-C Hub", 200 },
                    { 10, 299.99m, "Monitor", 40 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
