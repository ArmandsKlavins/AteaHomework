# XYZ Inc. Billing Application

Accepts orders over a REST API and pays them through mocked payment gateways. ASP.NET Core (.NET 10) backend, Angular 22 frontend.

## Running

Prerequisites: [.NET 10 SDK](https://dotnet.microsoft.com/download), [Node.js](https://nodejs.org/) 22+ with npm.

Backend (API on `http://localhost:5209`, Swagger UI at `http://localhost:5209/swagger`):

```bash
cd backend/src/Billing.Api
```

```bash
dotnet run
```

Frontend (`http://localhost:4200`), in a second terminal:

```bash
cd frontend
```

```bash
npm install && npm start
```

Tests (unit + integration, no running server needed), from the repository root:

```bash
dotnet test backend/Billing.slnx
```

### Configuration

None needed: the app runs out of the box with mock payment gateways.

The only setting is `Cors:AllowedOrigins` in `backend/src/Billing.Api/appsettings.json`, the browser origins allowed to call the API. It defaults to the Angular dev server (`http://localhost:4200`).

## Task requirements

| Requirement | How / where |
|---|---|
| Receive an order, forward it to the right payment gateway | `POST /api/orders` → `OrderService` resolves the gateway by `paymentGatewayId` and charges it |
| Receipt on success (order number, amount, timestamp, confirmation) | `201 Created` with `ReceiptResponse` (`orderNumber`, `amount`, `paidAt`, `transactionId`); also `GET /api/orders/{orderNumber}/receipt` |
| Clear error on failure | Every error is [ProblemDetails](https://datatracker.ietf.org/doc/html/rfc9457): `400` validation / unknown gateway, `402` declined, `409` duplicate, `500` unexpected |
| No double payment | The order number is claimed atomically (`ConcurrentDictionary.TryAdd` in `InMemoryOrderRepository`) before charging; a resubmit gets `409` |
| Multiple gateways, extensible | `IPaymentGateway` + `IPaymentGatewayResolver`; mocks `card` and `bank` in `Infrastructure/PaymentGateways/Mocks/`. New gateway = one class + one DI line in `Program.cs` |
| Unit tests | `backend/tests/Billing.UnitTests` (xUnit, Moq, `FakeTimeProvider`) |
| Clean, maintainable, extensible code | N-layer projects with one job each; dependencies behind interfaces and injected (gateways, repositories, services, `TimeProvider`); outcomes as result types, not exceptions. New gateways or storage plug in through DI without touching the logic layer |
| UI submits orders and displays results | Angular app in `frontend/`: order form with gateway picker, shows the receipt or the API's error message |
| Runs with `dotnet run` / `npm install && npm start` | See [Running](#running) |
| Bonus: logging, structured errors | Structured logs for every order outcome in `OrderService`; ProblemDetails everywhere, incl. unknown routes and `500` |
| Bonus: Swagger / OpenAPI | `/openapi/v1.json` and Swagger UI at `/swagger` (Development) |
| Bonus: retry logic | A declined order can be resubmitted with the same order number. Failed charges are deliberately not retried automatically (double-charge risk). See [Retries](#retries) for full info |
| Bonus: integration tests | `backend/tests/Billing.IntegrationTests` (`WebApplicationFactory`, real HTTP in memory) |
| Bonus: frontend validation | Reactive form validators mirroring `OrderRequest` (`frontend/src/app/app.ts`) |
| Bonus: Docker Compose | Not done |

## Project structure

```
backend/
  Billing.slnx
  src/
    Billing.Api/             # controllers, DI (Program.cs)
    Billing.Contracts/       # request/response DTOs with validation
    Billing.Logic/           # OrderService, ReceiptService, PaymentGatewayService, mappings
    Billing.Infrastructure/  # repositories (in memory), payment gateways (mocks)
    Billing.Domain/          # Order, Receipt
  tests/
    Billing.UnitTests/
    Billing.IntegrationTests/
frontend/src/app/            # App (form, receipt, errors), services/billing-api.ts, models/
```

Classic N-layer: `Api → Logic → Infrastructure → Domain`, with Contracts shared by Api and Logic. Orders and receipts are stored in memory and lost on restart.

The logic layer only depends on the `IOrderRepository` and `IReceiptRepository` interfaces, so storage can later move to a database (for example with Entity Framework Core) without touching Logic: add new repository implementations in `Billing.Infrastructure/Repositories/` and swap their DI registrations in `Program.cs`. The order repository's `TryAddAsync` must stay atomic (for example a unique key on the order number, returning `false` on a duplicate insert), because that is what prevents double payment.

## API

| Endpoint | Responses |
|---|---|
| `POST /api/orders` | `201` receipt (+ `Location` header) · `400` invalid / unknown gateway · `402` declined · `409` already processed · `500` charge failed |
| `GET /api/orders/{orderNumber}/receipt` | `200` receipt · `404` not paid |
| `GET /api/payment-gateways` | `200` `[{ "id": "card" }, { "id": "bank" }]` (unordered) |

Order request:

| Field | Rules |
|---|---|
| `orderNumber` | required, 1–50 letters, digits, `-`, `_` |
| `userId` | required, up to 100 characters |
| `payableAmount` | at least `0.01`, at most 2 decimals |
| `paymentGatewayId` | required, an ID from `GET /api/payment-gateways` (case-insensitive) |
| `description` | optional, up to 500 characters |

```bash
curl -X POST http://localhost:5209/api/orders \
  -H "Content-Type: application/json" \
  -d '{"orderNumber":"ORD-1","userId":"user-1","payableAmount":10.50,"paymentGatewayId":"card"}'
```

```json
{ "orderNumber": "ORD-1", "amount": 10.50, "paidAt": "2026-11-02T10:15:30.123+00:00", "transactionId": "card_3f2b9c..." }
```

More sample requests: `backend/src/Billing.Api/Billing.Api.http`.

## Payment gateways

| ID | Mock behaviour |
|---|---|
| `card` | approves every payment |
| `bank` | declines amounts over 1000 |

## Retries

An order is processed in three steps: resolve the gateway (a typo doesn't use up the order number) → store the order (claims the number) → charge. What a resubmit of the same order number gets depends on how the first attempt ended:

- **Declined**: the order is removed, so the number can be retried.
- **Paid**: `409`; a client that lost the response can recover its receipt with `GET /api/orders/{orderNumber}/receipt`.
- **Charge failed** (e.g. a gateway timeout): the payment may still have gone through, so the API returns `500`, logs the order for a manual check and keeps the number blocked (`409`). Failed charges are deliberately never retried automatically, because charging again could take the money twice.

With real gateways, retries become safe once each charge sends the order number as an idempotency key; then timeouts can be retried with [Polly](https://www.pollydocs.org/) (`Microsoft.Extensions.Http.Resilience`) on the gateway's `HttpClient`.
