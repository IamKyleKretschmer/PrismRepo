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
