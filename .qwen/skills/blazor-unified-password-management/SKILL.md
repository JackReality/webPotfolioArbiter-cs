---
name: blazor-unified-password-management
description: Unify password change (profile) and reset (email token) flows in a single Blazor page, using scoped DbContext to prevent concurrency errors and proper form submission handling.
source: auto-skill
extracted_at: '2026-06-07T17:00:00.000Z'
---

# Unified Password Management in Blazor

When implementing password changes and resets, avoid adding token columns to the `User` table. Instead, use a dedicated `EmailToken` table with a `purpose` column (e.g., 'recovery', 'confirm') to keep the `User` model clean and support multiple token types.

## Approach

1. **Model**: Create an `EmailToken` model mapped to the `email_tokens` table with `UserId`, `Token`, `Purpose`, and `ExpiresAt`.
2. **Service (Concurrency-Safe)**: Create a `TokenService` that injects `IServiceScopeFactory` instead of `AppDbContext` directly. Create a new scope for each database operation (`using var scope = _scopeFactory.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();`). This prevents "A second operation was started on this context instance" errors common in Blazor Server when multiple components initialize concurrently.
3. **Unified Page**: Use a single Razor page (e.g., `ResetPassword.razor`) that handles both flows:
   - Add multiple `@page` directives (e.g., `@page "/profil/changer-mdp"` and `@page "/reset-password"`).
   - **CRITICAL**: Do NOT use `@attribute [Authorize]` on this page. If a user accesses the reset link in a private window (not logged in), Blazor will intercept the request and redirect to `/login` *before* the page can read the token, breaking the flow. Handle authorization manually in `OnInitializedAsync` based on the presence of the token.
   - Use `[SupplyParameterFromQuery(Name = "token")]` to capture the token from the URL.
   - In `OnInitializedAsync`, check if the token is present. If yes, validate it via `TokenService` (mode "recovery"). If no, ensure the user is authenticated via `AuthenticationStateProvider` (mode "profile").
   - Conditionally render the "Current Password" field only when not in recovery mode (`@if (!_isRecoveryMode)`).
4. **Form Submission**: To guarantee 100% Blazor-side event handling and prevent *any* native browser form submission (which causes jarring full-page reloads and state loss), use MudBlazor components exclusively:
   - Use `<MudForm OnSubmit="MethodAsync">` OR a simple `<div>` wrapper.
   - Use `<MudButton ButtonType="ButtonType.Button" OnClick="MethodAsync">`. Do not use `type="submit"` with `@onclick` on native `<button>` elements inside a `<form>`.
5. **Security**: Always delete/invalidate the token after a successful password reset to prevent reuse.
6. **Development Workflow**: Always explicitly check the `dotnet watch` terminal output for "Build succeeded" before asking the user to test. A silent build failure (e.g., due to a syntax error) means the server serves an old, cached version of the site, leading to massive confusion.

## Why
- **Separation of concerns**: The `User` table remains lightweight and focused on profile data.
- **Blazor Server Stability**: Using `IServiceScopeFactory` isolates DbContext operations, preventing concurrency crashes during page initialization.
- **UX & Maintainability**: `@onsubmit:preventDefault` ensures smooth, single-page-application behavior without jarring full-page reloads. A single page reduces code duplication and provides a consistent user experience.