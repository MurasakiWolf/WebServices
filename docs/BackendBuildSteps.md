# WebService Backend — Steps Followed

> Reconstruction of how the backend under `src/` was built, from the empty
> repository (`f0d9c80`) up to and including the OpenAPI + typed-client work
> (`e416730`).
>
> Commands are written for **PowerShell** and are run from the **repository
> root** (the folder containing the solution file). Where a commit only records
> the _result_ of a tool (an EF migration, an NSwag-generated file), the command
> that produces that result is given so the state can be reproduced.

---

## Commit map

| #   | Commit                                     | Date               | Summary                                                            | Touches `src/` |
| --- | ------------------------------------------ | ------------------ | ------------------------------------------------------------------ | :------------: |
| 0   | `f0d9c80`                                  | 2026-08-24         | Initial commit — `.gitignore`, `README.md` only                    |       —        |
| 1   | `20fb7d1`                                  | 2026-08-24         | Add initial boilerplate (Clean Architecture skeleton)              |       ✅       |
| 2   | `6a37ba7`                                  | 2026-08-24         | Cleanup boilerplate                                                |       ✅       |
| 3   | `a0a93ef`                                  | 2026-08-25         | Add entity and EF boilerplate                                      |       ✅       |
| 4   | `66494c6`                                  | 2026-08-26         | Add first entity and initial migration                             |       ✅       |
| 5   | `668887e`                                  | 2026-08-27         | Add DB initializer (seed data)                                     |       ✅       |
| —   | `d5a0b05`, `6bf43a3`, `d738e9d`, `425a70c` | 2026-08-27/28      | `OpenApi.md` design-doc drafts (`openapi v1/v2/v3`)                |       —        |
| 6   | `2fe4d9c`                                  | 2026-08-28         | Rename solution, projects, folders and namespaces → **WebService** |       ✅       |
| 7   | `cc2e216`                                  | 2026-08-28         | Add Events controller (+ shared base controller)                   |       ✅       |
| —   | `cff6364`, `807bb12`                       | 2026-08-31 / 09-01 | Bruno integration tests (under `tests/`, not `src/`)               |       —        |
| 8   | `e416730`                                  | 2026-09-02         | Add OpenAPI document host + NSwag RPC client generation            |       ✅       |

Final project layout (after step 8):

```
WebService.slnx
.config/dotnet-tools.json          # local NSwag CLI
src/
  WebService.Api/                   # ASP.NET Core Web API (controllers + EF + seeding)
  WebService.Application/            # class library (placeholder, wired for references)
  WebService.Domain/                # entities (Event)
  WebService.Persistence/           # AppDbContext, migrations, DbInitializer
  WebService.OpenApi/               # standalone host that serves the Swagger doc
    Generated/WebServiceRpcClient.generated.cs   # generated (never hand-edited)
  nswag/WebService.nswag             # checked-in codegen config
  openapi/WebService.v1.json         # generated OpenAPI document (never hand-edited)
```

---

## Step 0 — Initial commit (`f0d9c80`)

Empty repository: only `.gitignore` (Visual Studio / .NET template) and a
one-line `README.md` (`# ActivitiesHub` — the project was originally called
_ActivitiesHub_; it is renamed to _WebService_ in step 6).

---

## Step 1 — Initial boilerplate (`20fb7d1`)

A four-project Clean Architecture skeleton plus a solution file.

```powershell
dotnet new sln -n ActivitiesHub

dotnet new webapi   -n API         -o src/API --use-controllers
dotnet new classlib -n Application -o src/Application
dotnet new classlib -n Domain      -o src/Domain
dotnet new classlib -n Persistence -o src/Persistence

dotnet sln add src/API/API.csproj `
               src/Application/Application.csproj `
               src/Domain/Domain.csproj `
               src/Persistence/Persistence.csproj

# reference graph: API -> Application -> (Domain, Persistence); Persistence -> Domain
dotnet add src/API/API.csproj                 reference src/Application/Application.csproj
dotnet add src/Application/Application.csproj  reference src/Domain/Domain.csproj
dotnet add src/Application/Application.csproj  reference src/Persistence/Persistence.csproj
dotnet add src/Persistence/Persistence.csproj  reference src/Domain/Domain.csproj
```

Details captured in the commit:

- All four projects target **`net10.0`** with `Nullable` and `ImplicitUsings`
  enabled. `API` uses `Microsoft.NET.Sdk.Web`; the libraries use
  `Microsoft.NET.Sdk`.
- `src/API/API.csproj` references `Microsoft.AspNetCore.OpenApi` `10.0.11`.
- The solution file is `ActivitiesHub.slnx` (XML `.slnx` format) with a single
  `/src/` solution folder.
- `API` still contains the unmodified template output: `Program.cs` with
  `AddControllers()` + `AddOpenApi()` + `MapOpenApi()` + `UseHttpsRedirection()`
  - `UseAuthorization()`, `Controllers/WeatherForecastController.cs`,
    `WeatherForecast.cs`, `API.http`, a two-profile `launchSettings.json`, and
    `appsettings*.json`.
- `Application`, `Domain`, `Persistence` each contain only a placeholder
  `Class1.cs`.

---

## Step 2 — Cleanup boilerplate (`6a37ba7`)

Strip the template down to what the project actually needs.

- Delete `src/API/API.http`.
- `src/API/Program.cs` — remove `AddOpenApi()` / `MapOpenApi()`,
  `UseHttpsRedirection()` and `UseAuthorization()`. What remains:

  ```csharp
  var builder = WebApplication.CreateBuilder(args);

  builder.Services.AddControllers();

  var app = builder.Build();

  if (app.Environment.IsDevelopment()) { }

  app.MapControllers();

  app.Run();
  ```

- `src/API/Properties/launchSettings.json` — drop the `http` profile; reduce the
  `https` profile to `applicationUrl: "https://localhost:5001"`.

---

## Step 3 — Entity and EF boilerplate (`a0a93ef`)

Introduce the first domain entity and wire up Entity Framework Core with SQLite.

```powershell
dotnet add src/Persistence/Persistence.csproj package Microsoft.EntityFrameworkCore.Sqlite  --version 10.0.11
dotnet add src/API/API.csproj                 package Microsoft.EntityFrameworkCore.Design  --version 10.0.11
```

- `src/Domain/Activity.cs` — new entity (replaces `Class1.cs`):

  ```csharp
  namespace Domain;

  public class Activity
  {
      public string Id { get; set; } = Guid.NewGuid().ToString();
      public required string Title { get; set; }
      public DateTime Date { get; set; }
      public required string Description { get; set; }
      public required string Category { get; set; }
      public bool IsCancelled { get; set; }
      public required string City { get; set; }
      public required string Venue { get; set; }
      public required string Latitude { get; set; }
      public required string Longitude { get; set; }
  }
  ```

- `src/Persistence/AppDbContext.cs` — new (replaces `Class1.cs`):

  ```csharp
  using Microsoft.EntityFrameworkCore;

  namespace Persistence;

  public class AppDbContext(DbContextOptions options) : DbContext(options)
  {
      public DbSet<Activity> Activities { get; set; }
  }
  ```

- `src/API/Program.cs` — register the context, reading the connection string
  from configuration:

  ```csharp
  using Microsoft.EntityFrameworkCore;
  using Persistence;
  // ...
  builder.Services.AddDbContext<AppDbContext>(opt =>
  {
      opt.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection"));
  });
  ```

---

## Step 4 — First entity rename + initial migration (`66494c6`)

- Rename the entity `Activity` → **`Event`** (`src/Domain/Event.cs`); update
  `AppDbContext` to expose `DbSet<Event> Events`.
- `src/API/appsettings.json` and `appsettings.Development.json` — add:

  ```json
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "SqliteConnection": "Data source=activitieshub.db"
  }
  ```

- Create the first EF Core migration:

  ```powershell
  dotnet tool install --global dotnet-ef            # if not already installed
  dotnet ef migrations add InitialCreate -p src/Persistence -s src/API
  ```

  This adds `src/Persistence/Migrations/20260826133339_InitialCreate.cs`
  (+ `.Designer.cs`) and `AppDbContextModelSnapshot.cs`, describing a single
  `Events` table (string `Id` PK, all string columns `NOT NULL`, `Date` as
  `TEXT`, `IsCancelled` as `INTEGER`).

---

## Step 5 — DB initializer / seed data (`668887e`)

- `src/Persistence/DbInitializer.cs` — new:

  ```csharp
  using Domain;

  namespace Persistence;

  public static class DbInitializer
  {
      public static async Task SeedDataAsync(AppDbContext context)
      {
          if (context.Events.Any()) return;

          var events = new List<Event> { /* 10 events: 5 past, 5 future */ };

          context.Events.AddRange(events);
          await context.SaveChangesAsync();
      }
  }
  ```

  The seed set is 10 Mexican-venue events, dated `DateTime.Now.AddMonths(-9 .. +8)`.

- `src/API/Program.cs` — after `builder.Build()`, apply migrations and seed on
  startup, inside a try/catch that logs through `ILogger<Program>`:

  ```csharp
  using var scope = app.Services.CreateScope();
  var services = scope.ServiceProvider;
  try
  {
      var context = services.GetRequiredService<AppDbContext>();
      await context.Database.MigrateAsync();
      await DbInitializer.SeedDataAsync(context);
  }
  catch (Exception ex)
  {
      var logger = services.GetRequiredService<ILogger<Program>>();
      logger.LogError(ex, "An error occurred during database migration.");
  }
  ```

> The intervening commits `d5a0b05` and `openapi v1/v2/v3`
> (`6bf43a3`, `d738e9d`, `425a70c`) only edit the root `OpenApi.md` design
> document — no `src/` changes. They are the written plan that step 8 implements.

---

## Step 6 — Rebrand to WebService (`2fe4d9c`)

A pure rename pass: **ActivitiesHub → WebService** across the solution, projects,
folders and namespaces. Nothing about behaviour changes.

| Before                                                              | After                                                                                                |
| ------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| `ActivitiesHub.slnx`                                                | `WebService.slnx`                                                                                    |
| `src/API/` · `API.csproj` · namespace `API`                         | `src/WebService.Api/` · `WebService.Api.csproj` · namespace `WebService.Api`                         |
| `src/Application/` · `Application.csproj`                           | `src/WebService.Application/` · `WebService.Application.csproj`                                      |
| `src/Domain/` · `Domain.csproj` · namespace `Domain`                | `src/WebService.Domain/` · `WebService.Domain.csproj` · namespace `WebService.Domain`                |
| `src/Persistence/` · `Persistence.csproj` · namespace `Persistence` | `src/WebService.Persistence/` · `WebService.Persistence.csproj` · namespace `WebService.Persistence` |

Also in this commit:

- Every `ProjectReference` repointed to the new paths; every `.slnx` entry
  updated.
- Migration re-created under the new namespace
  (`WebService.Persistence.Migrations`, entity key `WebService.Domain.Event`) and
  re-timestamped `20260828131904_InitialCreate`.
- Connection string database file → `Data source=WebService.db`.
- `README.md` → `# WebService`.
- Removed the leftover `Application/Class1.cs` and an unused
  `using System.ComponentModel.DataAnnotations;` in `Event.cs`.

Reproduce with (rough equivalent):

```powershell
Rename-Item ActivitiesHub.slnx WebService.slnx
Rename-Item src/API          src/WebService.Api
Rename-Item src/Application   src/WebService.Application
Rename-Item src/Domain       src/WebService.Domain
Rename-Item src/Persistence  src/WebService.Persistence
# rename each .csproj, fix namespaces + ProjectReferences + .slnx paths,
# then regenerate the migration:
dotnet ef migrations remove -p src/WebService.Persistence -s src/WebService.Api
dotnet ef migrations add InitialCreate -p src/WebService.Persistence -s src/WebService.Api
```

---

## Step 7 — Events controller (`cc2e216`)

- `src/WebService.Api/Controllers/WebServiceBaseController.cs` — shared base with
  the API route convention:

  ```csharp
  [Route("api/v1/[controller]")]
  [ApiController]
  public class WebServiceBaseController : ControllerBase { }
  ```

- `src/WebService.Api/Controllers/EventsController.cs` — the real endpoint,
  injecting `AppDbContext` directly:

  ```csharp
  public class EventsController(AppDbContext context) : WebServiceBaseController
  {
      [HttpGet]
      public async Task<ActionResult<IReadOnlyList<Event>>> GetEventsAsync()
          => await context.Events.ToListAsync();

      [HttpGet("{id}")]
      public async Task<ActionResult<Event>> GetEventDetailAsync(string id)
      {
          var result = await context.Events.FindAsync(id);
          if (result == null) return NotFound("The event was not found");
          return result;
      }
  }
  ```

- `WeatherForecastController` is re-parented onto `WebServiceBaseController`
  (drops its own `[ApiController]` / `[Route("[controller]")]`), so it now
  answers at `api/v1/WeatherForecast`.

> Commits `cff6364` / `807bb12` add Bruno integration tests under `tests/` —
> outside the `src/` scope of this document.

---

## Step 8 — OpenAPI document host + NSwag RPC client (`e416730`)

Implements the plan in `OpenApi.md`: a standalone host project that reflects over
the real controllers to emit an OpenAPI document, then an NSwag CLI pass that
turns that document into a typed C# client.

### 8a. Standalone doc-generation host

```powershell
dotnet new web -n WebService.OpenApi -o src/WebService.OpenApi
dotnet sln WebService.slnx add src/WebService.OpenApi/WebService.OpenApi.csproj --solution-folder src

dotnet add src/WebService.OpenApi/WebService.OpenApi.csproj reference src/WebService.Api/WebService.Api.csproj
dotnet add src/WebService.OpenApi/WebService.OpenApi.csproj package NSwag.AspNetCore                   --version 14.7.1
dotnet add src/WebService.OpenApi/WebService.OpenApi.csproj package Microsoft.AspNetCore.Mvc.NewtonsoftJson --version 10.0.11
dotnet add src/WebService.OpenApi/WebService.OpenApi.csproj package Newtonsoft.Json                    --version 13.0.4
dotnet add src/WebService.OpenApi/WebService.OpenApi.csproj package Moq                                --version 4.20.72
```

`src/WebService.OpenApi/Program.cs` — registers the NSwag document, loads the
`WebService.Api` assembly as an MVC application part, and serves the doc + UI:

```csharp
using System.Reflection;
using WebService.Api.Controllers;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddOpenApiDocument(document =>
{
    document.DocumentName = "WebService";
    document.Title = "WebServiceV1";
    document.Version = "1.0.0";
    document.DefaultResponseReferenceTypeNullHandling =
        NJsonSchema.Generation.ReferenceTypeNullHandling.NotNull;
});

var pluginAssembly = Assembly.GetAssembly(typeof(WeatherForecastController));
services.AddMvc()
    .AddApplicationPart(pluginAssembly!)
    .AddControllersAsServices()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
    });

var app = builder.Build();
app.UseOpenApi();
app.UseSwaggerUi();
app.MapControllers();
app.Run();
```

`src/WebService.OpenApi/Properties/launchSettings.json` uses a free port pair
(`https://localhost:5011;http://localhost:5010`) so it doesn't collide with the
main API on `5001`.

> `typeof(Program)` cannot be used for the assembly handle — WebService's
> `Program` is an internal top-level class in the global namespace. A public type
> such as `WeatherForecastController` is referenced instead.

### 8b. NSwag CLI as a local tool

```powershell
dotnet new tool-manifest
dotnet tool install nswag.consolecore --version 14.7.1
```

Produces `.config/dotnet-tools.json` with the `nswag` command.

### 8c. Codegen config

`src/nswag/WebService.nswag` (hand-authored, checked in). Input is the
not-yet-generated document; output is the `Generated/` folder:

```json
{
  "runtime": "Net100",
  "documentGenerator": {
    "fromDocument": { "url": "../openapi/WebService.v1.json" }
  },
  "codeGenerators": {
    "openApiToCSharpClient": {
      "generateClientClasses": true,
      "generateClientInterfaces": true,
      "generateExceptionClasses": true,
      "exceptionClass": "ApiException",
      "className": "{controller}RpcClient",
      "operationGenerationMode": "MultipleClientsFromOperationId",
      "namespace": "WebService.OpenApi.Client",
      "jsonLibrary": "NewtonsoftJson",
      "output": "../src/WebService.OpenApi/Generated/WebServiceRpcClient.generated.cs"
    }
  }
}
```

### 8d. Run the generation pipeline

```powershell
# 1. start the host
dotnet run --project src/WebService.OpenApi/WebService.OpenApi.csproj --no-launch-profile --urls http://127.0.0.1:5011

# 2. fetch the document (separate terminal)
Invoke-WebRequest -Uri http://127.0.0.1:5011/swagger/WebService/swagger.json -OutFile src/openapi/WebService.v1.json

# 3. stop the host
Get-Process -Name "WebService.OpenApi" | Stop-Process -Force

# 4. generate the client
dotnet tool run nswag run src/nswag/WebService.nswag

# 5. confirm everything compiles
dotnet build WebService.slnx
```

Committed generated output:

- `src/openapi/WebService.v1.json` — OpenAPI 3.0 doc; paths `/api/v1/Events`,
  `/api/v1/Events/{id}`, `/api/v1/WeatherForecast`, plus the `Event` schema.
- `src/WebService.OpenApi/Generated/WebServiceRpcClient.generated.cs` (~774 lines)
  — `EventsRpcClient` / `WeatherForecastRpcClient` with their interfaces, an
  `ApiException` hierarchy, and the `Event` DTO, all in namespace
  `WebService.OpenApi.Client`.

Both files are **generated** — never hand-edited; re-run steps 8d.1–8d.4 to
refresh them.

Also added in this commit: `.vscode/settings.json`, and the design doc moved to
`docs/OpenApi.md`.

---

## What is generated vs. hand-maintained

| Path                                                                 | Origin                     | Maintenance                          |
| -------------------------------------------------------------------- | -------------------------- | ------------------------------------ |
| `src/WebService.*/**/*.cs` (non-migration), `*.csproj`, `Program.cs` | Hand-written               | Edit directly                        |
| `src/WebService.Persistence/Migrations/**`                           | `dotnet ef migrations add` | Regenerate; don't hand-edit          |
| `src/nswag/WebService.nswag`                                         | Hand-authored              | Edit for codegen config changes only |
| `src/openapi/WebService.v1.json`                                     | NSwag (pipeline 8d.2)      | **Never hand-edit**                  |
| `src/WebService.OpenApi/Generated/*.generated.cs`                    | NSwag (pipeline 8d.4)      | **Never hand-edit**                  |
