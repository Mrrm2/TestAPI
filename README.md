# TestAPI - Idempotency Proof of Concept
This repository demonstrates an implementation of idempotency in a simple ASP.NET Core Web API. The goal is to provide a reusable mechanism that prevents duplicate processing of the same request, using a custom `IdempotencyAttribute` and in-memory caching. This implementation is particularly useful for APIs where it is crucial to avoid executing the same operation multiple times, such as order placement, payment processing, or resource creation.

## Features
- Custom `IdempotencyAttribute` that can be applied to any controller method to enforce idempotency.
- In-memory caching to store and validate idempotency keys.
- Custom response message when a duplicate request is detected.
- Easy-to-use and reusable design.

## Getting Started
### Prerequisites
- .NET 8 SDK
- IDE such as Visual Studio or Visual Studio Code
- Postman or any other API testing tool for sending requests to the API

## Cloning the Repository
Clone the repository to your local machine:

```
git clone https://github.com/Mrrm2/TestAPI.git
cd TestAPI
```

## Setting up the Project
Open the solution in your preferred IDE (e.g., Visual Studio or Visual Studio Code).

Restore the necessary dependencies:
```dotnet restore```

Build the solution:
```dotnet build```

### Running the API
Run the application:
```dotnet watch```
.NET will give you a link of where the API is being hosted once it builds the project.

### API Endpoints
1. Create Order
- Description: Creates a new order using the provided details in the request body.
- URL: `https://localhost:PORT/api/orders` (The port .NET puts you on)
- Method: POST
- Headers:
    - `Idempotency-Key`: A unique key to ensure idempotency. If the same key is used within the cache duration, a duplicate response is returned.
    - `Content-Type`: `application/json`

- Request Body:
```
json

{
  "productId": 1,
  "quantity": 10,
  "status": "Pending"
}
```

- Response:

```
json

{
  "message": "Order created successfully",
  "orderId": 1
}
```

- Duplicate Request Response:
```
json

{
  "Message": "This order has already been placed.",
  "CachedResponse": {
    "message": "Order created successfully",
    "orderId": 1
  }
}
```

2. Get Orders
- Description: Retrieves the list of all orders that have been placed.
- URL: `https://localhost:PORT/api/orders` (The port .NET puts you on)
- Method: GET
- Response:
```
json
[
  {
    "id": 1,
    "productId": 1,
    "quantity": 10,
    "status": "Pending",
    "orderDate": "2024-10-01T15:43:02.123Z"
  }
]
```
## Understanding the Idempotency Mechanism
The core logic of idempotency is implemented in the `IdempotencyAttribute` class, located in the `Attributes` folder. This class checks incoming requests for an `Idempotency-Key` header and uses in-memory caching to determine if the request has been previously processed.

### How It Works
1. When a request with an `Idempotency-Key` is received, the attribute checks if the key is present in the cache.
2. If the key is found, it returns a custom response indicating that the request has already been processed, along with the cached response data.
3. If the key is not found, the request is processed, and the result is cached using the provided Idempotency-Key for future reference.

### Cache Duration
The cache duration is currently set to 5 minutes (300 seconds) in the `IdempotencyAttribute` class:
```Cache.Set(idempotencyKey, okResult.Value, TimeSpan.FromMinutes(5));```

### Why 5 Minutes?
- Reasoning: A 5-minute cache duration is a common choice for idempotency in scenarios where network instability or client retries may occur within a short timeframe. This duration ensures that if a client accidentally resends the same request due to a timeout or error, the server can handle it gracefully without creating duplicate resources.

- Adjustability: You can adjust this duration based on your application's specific requirements. For example, for financial transactions or order processing systems where retries might happen over a longer duration, you may want to set the cache duration to 15 minutes or even several hours.

Modifying the Cache Duration
To change the cache duration, update the `TimeSpan` value in the `IdempotencyAttribute` class:

```Cache.Set(idempotencyKey, okResult.Value, TimeSpan.FromMinutes(10)); // Change 10 to the desired number of minutes```

## Testing the API
### Testing with Postman
1. Create an Order:
    - Send a POST request to `https://localhost:PORT/api/orders` with a unique Idempotency-Key.
    
    - Headers:
        - `Idempotency-Key: unique-key-123`
        - `Content-Type: application/json`

- Body:
```
json

{
  "productId": 1,
  "quantity": 10,
  "status": "Pending"
}
```

2. Duplicate Order Request:
    - Send the same POST request again with the same `Idempotency-Key`.
    - Verify that the response is:

```
json

{
  "Message": "This order has already been placed.",
  "CachedResponse": {
    "message": "Order created successfully",
    "orderId": 1
  }
}
```

3. New Order with a Different Key:
    - Send a new POST request with a different Idempotency-Key.
    - A new order should be created, and a different orderId should be returned.
    
4. Get Orders:
    - Send a GET request to `https://localhost:PORT/api/orders` to see all placed orders.

### Further Enhancements
- Use a persistent cache like Redis store idempotency keys across multiple instances of the application (for next stage).
- Implement business-level checks to prevent duplicates even if idempotency keys expire.
- Add authentication and authorization mechanisms to ensure only authenticated users can access and create orders.

## Conclusion
This repository demonstrates how to implement idempotency in a simple ASP.NET Core Web API using a custom attribute and in-memory caching. It provides a basic yet effective solution to handle duplicate requests and ensure that sensitive operations are not performed more than once.
