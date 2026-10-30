# XYZ Inc. Billing Application

A billing application that accepts orders over a REST API and processes them through mocked payment gateways.

> Work in progress: only the backend exists so far, and order processing is a placeholder that returns the submitted order.

## Tech stack

- Backend: ASP.NET Core Web API (.NET 10, controllers)

## Project structure

```
Billing.slnx
src/
  Billing.Api/
    Controllers/OrdersController.cs   # POST /api/orders
    Program.cs                        # DI registrations
  Billing.Logic/                      # business logic, no ASP.NET dependency
    Models/Order.cs                   # order payload + validation rules
    Services/OrderService.cs          # order processing
    Repositories/InMemoryOrderRepository.cs
```

Orders are stored in memory, keyed by order number, and are lost when the API restarts.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Running the backend

```bash
dotnet run --project src/Billing.Api --launch-profile http
```

The API listens on `http://localhost:5209`. In Development, the OpenAPI document is served at `http://localhost:5209/openapi/v1.json`.

No configuration or environment variables are required.

## API

### `POST /api/orders`

Submits an order for processing.

Request body:

| Field              | Type    | Required | Rules            |
|--------------------|---------|----------|------------------|
| `orderNumber`      | string  | yes      | not empty        |
| `userId`           | string  | yes      | not empty        |
| `payableAmount`    | decimal | yes      | at least `0.01`  |
| `paymentGatewayId` | string  | yes      | not empty        |
| `description`      | string  | no       |                  |

Example:

```bash
curl -X POST http://localhost:5209/api/orders \
  -H "Content-Type: application/json" \
  -d '{"orderNumber":"ORD-1","userId":"user-1","payableAmount":10.50,"paymentGatewayId":"gateway-a","description":"Test order"}'
```

Responses:

- `200 OK`: the submitted order (placeholder until payment processing is implemented)
- `400 Bad Request`: validation errors as [ProblemDetails](https://datatracker.ietf.org/doc/html/rfc9457)
- `409 Conflict`: an order with the same `orderNumber` has already been processed

You can also send the sample request in `src/Billing.Api/Billing.Api.http` from Visual Studio, Rider or VS Code (REST Client).
