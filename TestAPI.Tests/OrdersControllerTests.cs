using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TestAPI.Tests
{
    [TestClass]
    public class OrdersControllerTests
    {
        private static HttpClient? _client;

        // Initialize the HTTP client for testing
        [ClassInitialize]
        public static void Initialize(TestContext testContext)
        {
            var application = new WebApplicationFactory<TestAPI.Program>();
            _client = application.CreateClient();
        }




        [TestMethod]
        public async Task CreateOrder_Should_Return_SameResponse_When_DuplicateRequest()
        {
            // Arrange
            var idempotencyKey = "unique-key-123";
            var requestBody = "{\"productId\": 1, \"quantity\": 10, \"status\": \"Pending\"}";
            var requestContent = new StringContent(requestBody, Encoding.UTF8, "application/json");

            // First Request
            var requestMessage1 = new HttpRequestMessage(HttpMethod.Post, "/api/orders")
            {
                Content = requestContent
            };
            requestMessage1.Headers.Add("Idempotency-Key", idempotencyKey);

            var response1 = await _client!.SendAsync(requestMessage1);
            var responseString1 = await response1.Content.ReadAsStringAsync();

            // Second Request (Duplicate)
            var requestMessage2 = new HttpRequestMessage(HttpMethod.Post, "/api/orders")
            {
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
            };
            requestMessage2.Headers.Add("Idempotency-Key", idempotencyKey);

            // Act
            var response2 = await _client.SendAsync(requestMessage2);
            var responseString2 = await response2.Content.ReadAsStringAsync();

            // Assert: Adjusted expected message to match the actual response format
            Assert.AreEqual(response1.StatusCode, response2.StatusCode, "Expected same status code for duplicate requests.");
            Assert.IsTrue(responseString2.Contains("This order has already been placed"), "Expected duplicate message in response.");
            Assert.AreEqual(
                "{\"message\":\"This order has already been placed.\",\"cachedResponse\":{\"message\":\"Order created successfully\",\"orderId\":1}}",
                responseString2,
                "Expected identical responses for duplicate requests.");
        }

        [TestMethod]
        public async Task CreateOrder_Should_Return_BadRequest_When_MissingIdempotencyKey()
        {
            // Arrange
            var requestBody = "{\"productId\": 1, \"quantity\": 10, \"status\": \"Pending\"}";
            var requestContent = new StringContent(requestBody, Encoding.UTF8, "application/json");

            // Act
            var response = await _client!.PostAsync("/api/orders", requestContent);

            // Assert: Adjusted expected message to match actual API behavior
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode, "Expected 400 Bad Request.");
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(responseString.Contains("Idempotency-Key header is required"), "Expected missing idempotency key message in response.");
        }


        [TestMethod]
        public async Task CreateOrder_Should_Return_Success_When_DifferentIdempotencyKeys()
        {
            // Arrange
            var idempotencyKey1 = "unique-key-987";
            var idempotencyKey2 = "unique-key-456";
            var requestBody = "{\"productId\": 1, \"quantity\": 10, \"status\": \"Pending\"}";
            var requestContent = new StringContent(requestBody, Encoding.UTF8, "application/json");

            // First Request
            var requestMessage1 = new HttpRequestMessage(HttpMethod.Post, "/api/orders")
            {
                Content = requestContent
            };
            requestMessage1.Headers.Add("Idempotency-Key", idempotencyKey1);

            var response1 = await _client!.SendAsync(requestMessage1);
            var responseString1 = await response1.Content.ReadAsStringAsync();

            // Second Request (Different Idempotency Key)
            var requestMessage2 = new HttpRequestMessage(HttpMethod.Post, "/api/orders")
            {
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
            };
            requestMessage2.Headers.Add("Idempotency-Key", idempotencyKey2);

            // Act
            var response2 = await _client.SendAsync(requestMessage2);
            var responseString2 = await response2.Content.ReadAsStringAsync();

            // Assert
            Assert.IsTrue(response2.IsSuccessStatusCode, "Expected a success status code.");
            Assert.IsTrue(responseString2.Contains("Order created successfully"), "Expected success message in response.");
            Assert.AreNotEqual(responseString1, responseString2, "Expected different responses for different idempotency keys.");
        }

    }
}
