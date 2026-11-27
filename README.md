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

## Health

`GET /health` needs no token and returns no item data. It returns `200 Healthy` when the data file can be opened and written, and `503 Unhealthy` otherwise (for example, the file is read-only or its directory can't be created). Use it for liveness and readiness.

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
