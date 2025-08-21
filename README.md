# GK.Talks

## Overview
GK.Talks is a small .NET 9 minimal API that demonstrates:
- a Speaker aggregate and Session value object,
- a registration flow implemented in an app service,
- lightweight dependency per proj,
- publishing domain events via MediatR,
- API error handling mapped to ProblemDetails.

## Repo layout
- src/GK.Talks.Api — Minimal API, middleware, OpenAPI.
- src/GK.Talks.Application — Application layer, commands/handlers/services.
- src/GK.Talks.Core — Domain: aggregates, value objects, events, exceptions.
- src/GK.Talks.Infrastructure — EF Core, strategies, fee calculator.
- tests/GK.Talks.Tests — NUnit tests and WebApplicationFactory integration tests. 

## SQLite (development)
- Default dev conn is SQLite in-mem.

Notes:
- For integration tests, SQLite in-mem with open conn per fixture so schema persists for the test lifetime.

## Tests
- Run locally:
  - Build solution: `dotnet build GK.Talks.sln`
  - Run API: `dotnet run --project src/GK.Talks.Api`
  - Run tests: `dotnet test tests/GK.Talks.Tests --logger "trx;LogFileName=test-results.trx"`

## CI notes
- The CI:
  - builds and tests the solution,
  - runs formatting checks and CodeQL,
  - uploads test TRX artifacts.

## Contributing / Next steps
- Add/expand tests covering more test coverage on domain rules, middleware problem mapping.

## Declaration
This readme was refined by Msft Copilot.