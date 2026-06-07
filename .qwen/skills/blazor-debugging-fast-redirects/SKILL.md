---
name: blazor-debugging-fast-redirects
description: When debugging Blazor pages that redirect immediately after an action, check the terminal logs, not the browser UI, for Console.WriteLine output.
source: auto-skill
extracted_at: '2026-06-07T17:44:47.738Z'
---

# Debugging Fast-Redirecting Blazor Pages

When a Blazor page performs an action (like a successful password reset) and immediately redirects to another page (e.g., `/login`), any UI-bound error messages or success alerts will flash and disappear too quickly to read.

## Approach

1. **Use Terminal Logs**: Add `System.Console.WriteLine($"[DEBUG] ...")` statements in the C# code behind the action (e.g., in the Service or the `@code` block).
2. **Check the Server Console**: The output will appear in the terminal where `dotnet watch` or `dotnet run` is executing, **not** in the browser's developer console or UI. Terminal logs persist across page navigations.
3. **Inspect the Output**: Read the last few lines of the terminal output to determine exactly where the logic succeeded or failed (e.g., "Utilisateur introuvable" vs "SaveChangesAsync: 0 lignes affectées").

## Why
- **Visibility**: Browser UI state is destroyed on full-page redirects or rapid Blazor navigation, making transient messages impossible to read.
- **Persistence**: The server terminal maintains a continuous log of the application's lifecycle, providing a reliable source of truth for debugging backend logic and database interactions.

## How to apply
When instructing the user to check for debug output on a fast-redirecting page, explicitly state: "Regardez la fenêtre du terminal (la console noire où `dotnet watch` tourne), pas le navigateur."