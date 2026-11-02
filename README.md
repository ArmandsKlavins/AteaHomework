# XYZ Inc. Billing Application

A billing application that accepts orders over a REST API and processes them through mocked payment gateways.

> Work in progress: only the backend exists so far.

## Tech stack

- Backend: ASP.NET Core Web API (.NET 10, controllers)

## Project structure

```
Billing.slnx
src/
  Billing.Api/
    Controllers/OrdersController.cs   # POST /api/orders, GET /api/orders/{orderNumber}/receipt
    Program.cs                        # DI registrations
  Billing.Contracts/                  # DTOs: what clients send and receive, grouped by feature
    Orders/OrderRequest.cs            # order payload + validation rules
    Receipts/ReceiptResponse.cs       # returned for a paid order
  Billing.Logic/                      # business logic, no ASP.NET dependency
    Mappings/OrderMappings.cs         # request DTO → domain
    Mappings/ReceiptMappings.cs       # domain → response DTO
    Services/OrderService.cs          # order processing: resolve gateway, store, charge
    Services/ReceiptService.cs        # receipt creation and lookup
    Models/OrderResult.cs             # outcome: Processed / Duplicate / UnknownGateway / Declined
  Billing.Infrastructure/             # implementations that talk to the outside world (storage, payment providers)
    Repositories/IOrderRepository.cs
    Repositories/InMemoryOrderRepository.cs
    Repositories/IReceiptRepository.cs
    Repositories/InMemoryReceiptRepository.cs
    PaymentGateways/IPaymentGateway.cs
    PaymentGateways/PaymentGatewayResolver.cs  # finds a gateway by ID
    PaymentGateways/Models/PaymentResult.cs    # approved with transaction ID, or declined with a reason
    PaymentGateways/Mocks/CardPaymentGateway.cs
    PaymentGateways/Mocks/BankPaymentGateway.cs
  Billing.Domain/                     # domain models used by logic and storage
    Order.cs
    Receipt.cs                        # linked to its order by OrderNumber
```

Classic N-layer, not Clean Architecture: Logic depends on Infrastructure, and Infrastructure holds the interfaces and their implementations. Project references: `Api → Logic → Infrastructure → Domain`. Api also references Contracts and Infrastructure (to register the repository and gateways in DI), and Logic references Contracts and Domain directly. Api passes request DTOs to the logic layer, which maps them to domain models.

Orders and receipts are stored in memory in separate repositories, both keyed by order number, and are lost when the API restarts. A receipt is stored for every approved payment.

### Payment gateways

The order's `paymentGatewayId` picks the gateway (case-insensitive). Both gateways are mocks:

| ID     | Behaviour                  |
|--------|----------------------------|
| `card` | approves every payment     |
| `bank` | declines amounts over 1000 |

To add a gateway, implement `IPaymentGateway` and register it in `Program.cs` with `AddSingleton<IPaymentGateway, YourGateway>()`. Two gateways with the same ID make the first order fail with an error.

The service resolves the gateway first (an unknown ID doesn't use up the order number), then stores the order (which claims the order number atomically), then charges it. A declined order is removed again, so the same order number can be retried; an approved one returns `409` on any resubmit.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Running the backend

```bash
dotnet run --project src/Billing.Api --launch-profile http
```

The API listens on `http://localhost:5209`. In Development, the OpenAPI document is served at `http://localhost:5209/openapi/v1.json` and Swagger UI at `http://localhost:5209/swagger`.

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
  -d '{"orderNumber":"ORD-1","userId":"user-1","payableAmount":10.50,"paymentGatewayId":"card","description":"Test order"}'
```

Responses:

- `201 Created`: the payment was approved; returns the receipt, with `Location: /api/orders/ORD-1/receipt`:
  ```json
  {
    "orderNumber": "ORD-1",
    "amount": 10.50,
    "paidAt": "2026-11-02T10:15:30.123+00:00",
    "transactionId": "card_3f2b9c..."
  }
  ```
  `transactionId` is the payment confirmation from the gateway.
- `400 Bad Request`: validation errors, or an unknown `paymentGatewayId`, as [ProblemDetails](https://datatracker.ietf.org/doc/html/rfc9457)
- `402 Payment Required`: the gateway declined the payment; ProblemDetails with the title `Payment declined` and the reason in `detail`
- `409 Conflict`: an order with the same `orderNumber` has already been processed; if it was paid, fetch its receipt with the endpoint below

### `GET /api/orders/{orderNumber}/receipt`

Returns the receipt of a paid order, for example when the client lost the response to its `POST`.

Responses:

- `200 OK`: the receipt, same shape as above
- `404 Not Found`: no paid order has this order number (never submitted, declined, or still being charged), as ProblemDetails

You can also send the sample requests in `src/Billing.Api/Billing.Api.http` from Visual Studio, Rider or VS Code (REST Client).
