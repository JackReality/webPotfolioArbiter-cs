---
name: blazor-server-user-profile
description: Pattern for building user profile / password change pages in Blazor Server with cookie auth + MudBlazor
source: auto-skill
extracted_at: '2026-06-07T15:03:20.245Z'
---

## Profile page pattern (Blazor Server + cookie auth)

### Loading the current user

In an interactive Blazor Server page, get the logged-in user's ID from `AuthenticationStateProvider` (not from `HttpContext`):

```razor
@page "/profil"
@attribute [Authorize]
@inject AuthenticationStateProvider AuthStateProvider
@inject UserService UserService

@code {
    private User? _user;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var idClaim = authState.User.FindFirst(ClaimTypes.NameIdentifier);
        if (idClaim != null && ulong.TryParse(idClaim.Value, out var userId))
        {
            _user = await UserService.GetByIdAsync(userId);
        }
    }
}
```

### Loading data in the layout (MainLayout)

When the layout needs user data (e.g. avatar in the header), inject `AuthenticationStateProvider` + `UserService` and call `StateHasChanged()` after loading so the re-render picks up the new data:

```razor
@inject AuthenticationStateProvider AuthStateProvider
@inject UserService UserService

@code {
    private string? _userAvatarUrl;

    protected override async Task OnInitializedAsync()
    {
        Nav.LocationChanged += OnLocationChanged;
        await LoadUserAvatarAsync();
    }

    private async Task LoadUserAvatarAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var idClaim = authState.User.FindFirst(ClaimTypes.NameIdentifier);
        if (idClaim != null && ulong.TryParse(idClaim.Value, out var userId))
        {
            var dbUser = await UserService.GetByIdAsync(userId);
            _userAvatarUrl = dbUser?.AvatarUrl;
            StateHasChanged();   // <-- important: triggers re-render with avatar
        }
    }
}
```

### MudSelectItem string values — Razor CS0103 gotcha

When using `MudSelectItem` with string `Value` attributes, **always** use `@("...")` syntax:

```razor
<!-- ❌ FAILS: CS0103 "fr does not exist in current context" -->
<MudSelectItem Value="fr">Français</MudSelectItem>

<!-- ✅ WORKS -->
<MudSelectItem Value="@("fr")">Français</MudSelectItem>
```

Without `@()`, Razor treats `fr` as a C# variable name, not a string literal.

### Password change flow

The pattern for changing a password (requires verifying the current password first):

```csharp
// In UserService:
public async Task<bool> VerifierMotDePasseActuelAsync(ulong id, string motDePasse)
{
    var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
    if (user is null) return false;
    return BCrypt.Net.BCrypt.Verify(motDePasse, user.PasswordHash);
}

public async Task<bool> ChangerMotDePasseAsync(ulong id, string nouveauMotDePasse)
{
    var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
    if (user is null) return false;
    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(nouveauMotDePasse);
    await _db.SaveChangesAsync();
    return true;
}
```

### Project conventions applied

- **Single-file pages**: markup + `@code` in one `.razor` file
- **Service owns DB logic**: pages inject `UserService`, never touch `DbContext` directly
- `@bind-Value` for two-way binding; explicit `SaveChangesAsync()` on button click
- Translations in centralized `.resx` files via `IStringLocalizer<SharedResource>`

### Read-only fields — plain text, not disabled inputs

When a field cannot be edited by the user (e.g. email pending confirmation), display it as **plain text** (`<MudText>@_user.Email</MudText>`), **never** as a disabled/greyed-out `<input>`. A disabled input confuses the user into thinking they should be able to edit it. Only use `<input>` / `@bind` for fields that are actually modifiable.

### Language tabs — use language names, not codes or flags

When building language switcher tabs, use the full language name ("Français", "English", "Español"), not country codes ("FR", "EN", "ES") or flag emojis. The user finds codes confusing and flag emojis unnecessary.

### Do NOT move files or restructure without asking

The project has a fixed directory convention:
- Pages → `Components/Pages/`
- Shared components → `Components/Shared/`
- Layouts → `Components/Layout/`
- `_Imports.razor` → `Components/_Imports.razor`

**Never** move `.razor` files to a new directory (e.g. `Pages/` at project root) or rename directories without explicit user approval. This breaks the build (namespace/import resolution) and causes confusion.
