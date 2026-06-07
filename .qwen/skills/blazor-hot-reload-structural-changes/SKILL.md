---
name: blazor-hot-reload-structural-changes
description: Handle Blazor `dotnet watch` hot reload limitations when adding `@page` directives or modifying component structure, which requires a manual restart.
source: auto-skill
extracted_at: '2026-06-07T17:15:00.000Z'
---

# Blazor Hot Reload Limitations (Structural Changes)

Blazor's `dotnet watch` hot reload is powerful for CSS and minor C# logic changes, but it **cannot** apply structural changes to Razor components on the fly. 

## Common Triggers for Hot Reload Failure
- Adding or removing an `@page` directive (e.g., adding `@page "/reset-password"` to an existing component).
- Changing the base class or adding new `@inject` directives that alter the component's fundamental structure.
- Adding new abstract or overriding inherited methods (ENC0023 error).

## Symptoms
- The terminal shows: `dotnet watch 🔥 Restart is needed to apply the changes.` or `error ENC0023`.
- The browser may show a blank page, a circuit error, or fail to route to the newly added `@page` URL.
- Subsequent `dotnet build` commands may fail with `MSB3026: The process cannot access the file... because it is being used by another process` (because the running `dotnet watch` process has locked the `.exe`/`.dll` files).

## Resolution
1. Go to the terminal running `dotnet watch`.
2. Press `Ctrl + C` to gracefully stop the process.
3. Run `dotnet watch` again to restart the development server with the new structural changes compiled in.

## Why
Hot reload works by injecting IL (Intermediate Language) deltas into the running process. Structural changes to the component's metadata (like routing tables or dependency injection signatures) require a full recompilation and process restart to be registered by the Blazor router and DI container.