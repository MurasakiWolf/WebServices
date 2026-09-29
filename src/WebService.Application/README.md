# WebService.Application

**This project is currently an unused placeholder.** It contains no `.cs`
files — only its `.csproj`, which references `WebService.Domain` and
`WebService.Persistence`.

## Why it exists

`WebService.Api` references only `WebService.Application` (not `Domain` or
`Persistence` directly). Since .NET project references are transitive, `Api`
reaches `Persistence`/`Domain` _through_ this project. It's a structural
stand-in for an application/service layer that the Clean-Architecture-shaped
solution layout implies should exist, but doesn't yet — controllers in
`WebService.Api` currently call `AppDbContext` from `WebService.Persistence`
directly. See
[`docs/Architecture.md`](../../docs/Architecture.md#architectural-pattern)
for the full picture.

## If you're adding backend logic

Before adding a service, use-case handler, or DTO/mapping type here, note
that today nothing in the codebase expects this layer to have content —
`Api` bypasses it entirely. Introducing real code here is a deliberate
architectural change (start routing controller logic through it), not just
"the obvious place to put a class." If that's not the intent, prefer adding
logic where the existing pattern already puts it (directly in the
`WebService.Api` controllers, alongside `AppDbContext` usage).
