# WebService

In this repository we will work the web services course and develop a full stack web service. ICI 2026

School project (ICI 2026 01): a .NET 10 backend with a React frontend, kept as
two independent apps in one repo — there is no shared build.

| Path                         | What it is                                                                | README                                         |
| ---------------------------- | ------------------------------------------------------------------------- | ---------------------------------------------- |
| `src/WebService.Api`         | ASP.NET Core Web API — the only backend piece in the runtime request path | [README](src/WebService.Api/README.md)         |
| `src/WebService.Application` | MediatR commands/queries and handlers (the use cases)                     | [README](src/WebService.Application/README.md) |
| `src/WebService.Domain`      | The `Event` entity                                                        | [README](src/WebService.Domain/README.md)      |
| `src/WebService.Persistence` | EF Core `AppDbContext`, migrations, seed data (SQLite)                    | [README](src/WebService.Persistence/README.md) |
| `src/WebService.OpenApi`     | Standalone host that generates the OpenAPI document and typed client      | [README](src/WebService.OpenApi/README.md)     |
| `web/`                       | React 19 + Vite + MUI single-page app                                     | [README](web/README.md)                        |
| `tests/`                     | NUnit unit tests and a Bruno HTTP collection                              | see the Api README                             |
| `docs/`                      | Architecture write-ups and course-material guides                         | [Architecture](docs/Architecture.md)           |

## How the pieces connect

```
web (React)  --HTTP-->  WebService.Api  --MediatR-->  WebService.Application  -->  WebService.Persistence  -->  SQLite
                         controllers                   handlers                    AppDbContext               WebService.db
                                                             \__ WebService.Domain (Event) used by every layer
WebService.OpenApi --loads Api assembly--> OpenAPI JSON --NSwag--> typed client   (build-time only)
```

- A controller in `WebService.Api` builds a MediatR command or query and sends
  it through `Mediator` (from `WebServiceBaseController`).
- The matching handler in `WebService.Application` uses `AppDbContext`
  directly (no repository layer) and returns `Event` objects, which are also
  the API response shape.
- `WebService.Api` references only `WebService.Application`; it reaches
  `Persistence` and `Domain` through that project reference.

## Quick start

```powershell
# backend (https://localhost:5001)
dotnet run --project src/WebService.Api

# frontend (https://localhost:3000)
cd web
npm install
npm run dev
```

Other commands (build, tests, EF migrations, OpenAPI regeneration) are listed
in the READMEs above and in [`CLAUDE.md`](CLAUDE.md).
