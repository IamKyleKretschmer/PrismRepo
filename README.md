# To-Do List API

An HTTP service for creating, listing and completing to-do items. The API contract is in [specs/todo-list-api/contracts/todo-api-v1.md](specs/todo-list-api/contracts/todo-api-v1.md).

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0). `global.json` pins the SDK line.

## Build, run and test

```sh
dotnet build TodoApi.slnx
dotnet run --project src/TodoApi
dotnet test tests/TodoApi.UnitTests
dotnet test tests/TodoApi.AcceptanceTests
```

`dotnet test TodoApi.slnx` runs both suites.

## Configuration

The service refuses to start, naming each missing setting, unless all of these are set. Set them as environment variables (shown) or in any other ASP.NET Core configuration source. Never commit secret values.

| Environment variable | Meaning |
| -------------------- | ------- |
| `Storage__DataFilePath` | Path of the SQLite data file. It is created on first start. It must be on persistent **local** storage, never network or ephemeral storage. |
| `Authentication__Issuer` | Expected `iss` of Vibranium JWTs. |
| `Authentication__Audience` | Expected `aud` of Vibranium JWTs. |
| `Authentication__Authority` | Signing-key source for deployed environments: the issuer's base URL, whose OpenID Connect metadata publishes the signing keys. |
| `Authentication__SigningKey` | Signing-key source for local and test runs only: a base64 symmetric key of at least 256 bits. |

Set exactly one of `Authentication__Authority` and `Authentication__SigningKey`.

## Authentication

Every `/v1` request needs a Vibranium JWT:

```sh
curl -H "Authorization: Bearer $TOKEN" http://localhost:5208/v1/todos
```

- **Mechanism**: the approved Vibranium JWT profile. The token must be signed by the configured key source, match `Authentication__Issuer` and `Authentication__Audience`, be unexpired (1 minute clock skew), and carry a `sub` claim.
- **Identity source**: the `sub` claim only. The `ntx-user-id` header is never read, because no approved trusted path sits in front of this service.
- **Authorisation**: any authenticated caller may use the shared list. There are no further permission checks.
- **Rejections**: a missing, expired, wrongly signed, wrong-issuer or wrong-audience token, or one without `sub`, gets `401` with code `unauthenticated`, and nothing changes.
- **Open endpoint**: only `/health` is reachable without a token.
- **Local and test runs**: set `Authentication__SigningKey` to a base64 key of at least 256 bits, and sign HS256 tokens with it using the same issuer and audience. The acceptance tests do this with a key that exists only in the test host. Never use a symmetric key in a deployed environment; set `Authentication__Authority` instead.

## Health

`GET /health` needs no token and returns no item data. It returns `200 Healthy` when the data file can be opened and written, and `503 Unhealthy` otherwise (for example, the file is read-only or its directory can't be created). Use it for liveness and readiness.

## Logs and metrics

Logs are JSON lines on stdout. Every request produces one entry from `TodoApi.Api.RequestLoggingMiddleware` with:

| Field | Meaning |
| ----- | ------- |
| `CorrelationId` (scope) | The caller's `X-Correlation-ID` header if it is 1–64 characters of `A-Z a-z 0-9 . _ -`, otherwise the trace ID. Echoed in the `X-Correlation-ID` response header, and present on every log entry written during the request. |
| `Method`, `Route` | HTTP method and route template (for example `/v1/todos/{id}/complete`), never the raw path. Unmatched paths log as `(unmatched)`. |
| `StatusCode`, `ElapsedMilliseconds` | Response status and duration. |
| `ErrorCode` | The contract's error `code`, on error responses only. |

At `Debug` level a second entry, `Caller {CallerReference}`, records a 16-character SHA-256 prefix of the token's `sub`, so one caller's requests can be correlated without logging their identity.

**Never logged**: request or response bodies (so no to-do titles, which may contain personal data), headers, tokens, or the raw `sub`.

**Metrics**: ASP.NET Core's built-in `http.server.request.duration` histogram (meter `Microsoft.AspNetCore.Hosting`) gives request count and latency, tagged with `http.route`, `http.request.method` and `http.response.status_code`. It follows OpenTelemetry semantic conventions; attach any OpenTelemetry exporter to collect it.

## CI

[`.github/workflows/ci.yml`](.github/workflows/ci.yml) runs on every pull request and every push to `main`/`master`: restore, build, unit tests, acceptance tests, then publishes the service package as the `todo-api` artefact. Make its **Build and test** job a required status check in branch protection so that a failing run blocks merging. The pipeline needs no secrets: acceptance tests use a signing key that exists only inside the test host.

## Layout

| Path | Contents |
| ---- | -------- |
| `src/TodoApi/Api/` | HTTP boundary: endpoints, authentication, error mapping |
| `src/TodoApi/Items/` | Item rules (title validation, completion). No HTTP or storage dependencies |
| `src/TodoApi/Persistence/` | Embedded SQLite store, transactions, ordering |
| `tests/TodoApi.UnitTests/` | xUnit unit tests |
| `tests/TodoApi.AcceptanceTests/` | xUnit black-box tests against the in-process host |
