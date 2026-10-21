# WebService.Persistence

The data-access layer: EF Core `DbContext`, migrations, and seed data.
Depends only on `WebService.Domain`.

## Contents

- **`AppDbContext.cs`** — `DbSet<Event> Events`. Configured for SQLite
  (`UseSqlite`) by whichever host wires it up (`WebService.Api`'s
  `Program.cs`, or `GlobalTestSetup` in the unit tests) — this project
  itself only declares the `Microsoft.EntityFrameworkCore.Sqlite` package
  reference, it doesn't hardcode a connection string.
- **`DbInitializer.cs`** — `SeedDataAsync(AppDbContext)`. Idempotent: returns
  immediately if `context.Events.Any()`. Otherwise inserts 10 fixed events
  at Mexican venues, dated across roughly nine months in the past to eight
  months in the future (`DateTime.Now.AddMonths(...)`), then
  `SaveChangesAsync()`.
- **`Migrations/`** — EF Core migration history. Currently a single
  `InitialCreate` migration describing one `Events` table (string `Id`
  primary key, all other columns `NOT NULL`, `Date` stored as `TEXT`,
  `IsCancelled` as `INTEGER`). **Generated — never hand-edit.**

## Who uses it

- **`WebService.Application`** references this project; every MediatR handler
  takes `AppDbContext` in its constructor and reads or writes `Events`
  through it. There is no repository layer in between.
- **`WebService.Api`** registers `AppDbContext` (`AddDbContext` +
  `UseSqlite`) and runs `MigrateAsync()` + `SeedDataAsync` at startup. It
  reaches this project through `WebService.Application`.
- **`tests/WebService.UnitTests`** references it directly and builds its own
  `AppDbContext` in `GlobalTestSetup`.

## Working with migrations

Run from the repo root, with `Persistence` as `-p` (the project containing
the `DbContext`) and `Api` as `-s` (the project with the connection string
and `Microsoft.EntityFrameworkCore.Design` package):

```powershell
dotnet ef migrations add <Name> -p src/WebService.Persistence -s src/WebService.Api
dotnet ef database update -p src/WebService.Persistence -s src/WebService.Api
```

Both `WebService.Api` (on every startup) and
`tests/WebService.UnitTests` (once, in `GlobalTestSetup`) call
`context.Database.MigrateAsync()` themselves — you generally don't need to
run `database update` manually against those; it's mainly useful for
inspecting the schema directly.

## Seed data caveat

Because `SeedDataAsync` only checks "is the table non-empty," it will not
re-seed or reconcile if the schema changes shape after data already exists
— it's a first-run bootstrap, not a general fixture/sync mechanism.
