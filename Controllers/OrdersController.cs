using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using TestAPI.Attributes;
using TestAPI.Models;

namespace TestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        // In-memory storage for orders
        private static readonly List<Order> Orders = new List<Order>();

        // Apply the Idempotency attribute to this POST method
        [HttpPost]
        [Idempotency]
        public IActionResult CreateOrder([FromBody] Order order)
        {
            // Validate the incoming order object
            if (order == null || order.ProductId <= 0 || string.IsNullOrEmpty(order.Status) || order.Quantity <= 0)
            {
                return BadRequest("Invalid order details. Please provide valid ProductId, Quantity, and Status.");
            }

            order.Id = Orders.Count > 0 ? Orders.Max(o => o.Id) + 1 : 1; // Auto-increment Order Id
            order.OrderDate = DateTime.UtcNow; // Set the order date to current UTC time
            Orders.Add(order); // Add order to in-memory list
            return Ok(new { Message = "Order created successfully", OrderId = order.Id });
        }

        [HttpGet]
        public IActionResult GetOrders()
        {
            return Ok(Orders);
        }
    }
}
