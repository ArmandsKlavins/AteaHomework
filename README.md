# XYZ Inc. Billing Application

A billing application that accepts orders over a REST API and processes them through mocked payment gateways.

> Work in progress: only the backend exists so far.

## Tech stack

- Backend: ASP.NET Core Web API (.NET 10, controllers)
- Tests: xUnit, Moq, `FakeTimeProvider`

## Project structure

```
Billing.slnx
src/
  Billing.Api/
    Controllers/OrdersController.cs   # POST /api/orders, GET /api/orders/{orderNumber}/receipt
    Controllers/PaymentGatewaysController.cs  # GET /api/payment-gateways
    Program.cs                        # DI registrations
  Billing.Contracts/                  # DTOs: what clients send and receive, grouped by feature
    Orders/OrderRequest.cs            # order payload + validation rules
    Validation/MaxDecimalPlacesAttribute.cs  # e.g. at most 2 decimals for money
    Receipts/ReceiptResponse.cs       # returned for a paid order
    PaymentGateways/PaymentGatewayResponse.cs  # an available gateway (ID only)
  Billing.Logic/                      # business logic, no ASP.NET dependency
    Mappings/OrderMappings.cs         # request DTO → domain
    Mappings/ReceiptMappings.cs       # domain → response DTO
    Mappings/PaymentGatewayMappings.cs  # gateway → response DTO
    Services/OrderService.cs          # order processing: resolve gateway, store, charge
    Services/ReceiptService.cs        # receipt creation and lookup
    Services/PaymentGatewayService.cs # lists available gateways
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
tests/
  Billing.UnitTests/                  # mirrors src: Logic/, Infrastructure/, Contracts/
  Billing.IntegrationTests/           # the whole API over HTTP, in memory (WebApplicationFactory)
```

Classic N-layer, not Clean Architecture: Logic depends on Infrastructure, and Infrastructure holds the interfaces and their implementations. Project references: `Api → Logic → Infrastructure → Domain`. Api also references Contracts and Infrastructure (to register the repository and gateways in DI), and Logic references Contracts and Domain directly. Api passes request DTOs to the logic layer, which maps them to domain models.

Orders and receipts are stored in memory in separate repositories, both keyed by order number, and are lost when the API restarts. A receipt is stored for every approved payment.

### Payment gateways

The order's `paymentGatewayId` picks the gateway (case-insensitive). Both gateways are mocks:

| ID     | Behaviour                  |
|--------|----------------------------|
| `card` | approves every payment     |
| `bank` | declines amounts over 1000 |

To add a gateway, implement `IPaymentGateway` and register it in `Program.cs` with `AddSingleton<IPaymentGateway, YourGateway>()`. Two gateways with the same ID make the first request that uses gateways fail with an error.

Gateway IDs are a public contract: clients send them as `paymentGatewayId` and use them as translation keys for display names. Add new gateways freely, but never rename or remove an ID without treating it as a breaking change.

The service resolves the gateway first (an unknown ID doesn't use up the order number), then stores the order (which claims the order number atomically), then charges it. A declined order is removed again, so the same order number can be retried; an approved one returns `409` on any resubmit. Once started, the charge runs to the end even if the client disconnects. If the charge fails with an error (for example a gateway timeout), the payment may or may not have gone through: the request returns `500`, the failure is logged with the order number and gateway, and the order number stays blocked (`409` on retry) until it's checked with the gateway by hand. This never charges twice.

Every order outcome is logged (structured, with the order number and gateway as fields) to the console: `Information` for charging, paid (with the transaction ID) and declined orders, `Warning` for unknown gateways and duplicates, and `Error` for failed charges.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Running the backend

```bash
dotnet run --project src/Billing.Api --launch-profile http
```

The API listens on `http://localhost:5209`. In Development, the OpenAPI document is served at `http://localhost:5209/openapi/v1.json` and Swagger UI at `http://localhost:5209/swagger`.

No configuration or environment variables are required.

## Running the tests

```bash
dotnet test Billing.slnx
```

The unit tests cover order processing (every outcome, including that an unknown gateway doesn't use up the order number and a declined order can be retried), receipts, gateway lookup and the mock gateways, the repositories (including concurrent submits of one order number), and request validation.

The integration tests run the real API in memory and call it over HTTP: every `POST /api/orders` outcome with its status code and ProblemDetails body, fetching the receipt from the `Location` header, a declined order retried on another gateway, a gateway that throws (`500` without internal details, then `409` on retry), the gateway list, and ProblemDetails for unknown routes. They need no running server or configuration.

## API

Every error response is a [ProblemDetails](https://datatracker.ietf.org/doc/html/rfc9457) object (`application/problem+json`) with `status`, `title`, an optional `detail` and a `traceId` that matches the server logs. That includes errors outside the endpoints below, such as unknown routes (`404`), wrong methods (`405`) and unexpected server errors (`500`, without internal details outside Development).

### `POST /api/orders`

Submits an order for processing.

Request body:

| Field              | Type    | Required | Rules                                                   |
|--------------------|---------|----------|---------------------------------------------------------|
| `orderNumber`      | string  | yes      | 1–50 characters: letters, digits, `-` and `_` only      |
| `userId`           | string  | yes      | not empty, up to 100 characters                         |
| `payableAmount`    | decimal | yes      | at least `0.01`, at most 2 decimal places               |
| `paymentGatewayId` | string  | yes      | an ID from `GET /api/payment-gateways`, up to 50 characters |
| `description`      | string  | no       | up to 500 characters                                    |

The order number's character set keeps it safe in the receipt URL (`/api/orders/{orderNumber}/receipt`).

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
- `500 Internal Server Error`: the gateway call failed, so whether the payment went through is unknown; the order number stays blocked (see above)

### `GET /api/orders/{orderNumber}/receipt`

Returns the receipt of a paid order, for example when the client lost the response to its `POST`.

Responses:

- `200 OK`: the receipt, same shape as above
- `404 Not Found`: no paid order has this order number (never submitted, declined, or still being charged), as ProblemDetails

### `GET /api/payment-gateways`

Lists the gateways an order can be paid through, in no particular order (clients sort them as they like):

```json
[{ "id": "card" }, { "id": "bank" }]
```

Only IDs are returned; the frontend decides how to name each gateway in each language it supports, and can fall back to the ID for one it has no translation for.

You can also send the sample requests in `src/Billing.Api/Billing.Api.http` from Visual Studio, Rider or VS Code (REST Client).
