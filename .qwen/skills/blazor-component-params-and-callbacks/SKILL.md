---
name: blazor-component-params-and-callbacks
description: Blazor component parameter patterns — EventCallback vs Func, admin-only gating, and tuple gotchas
source: auto-skill
extracted_at: '2026-06-07T16:30:00.000Z'
---

## Blazor component parameter patterns

### EventCallback with tuple parameters — CS1503 compilation error

**Problem:** Passing a method group to an `EventCallback` whose generic type is a tuple fails with `CS1503: conversion impossible de 'groupe de méthodes' en 'EventCallback'`:

```razor
<!-- Child component -->
@code {
    [Parameter]
    public EventCallback<(ulong Id, string Subject, string Html)> OnSave { get; set; }
}

<!-- Parent component -->
<ChildComponent OnSave="SaveAsync" />
@code {
    private async Task SaveAsync(ulong id, string subject, string html) { ... }
}
<!-- ❌ CS1503 -- Blazor can't convert the method group to EventCallback<(ulong, string, string)> -->
```

**Fix:** Use `Func<..., Task>` instead of `EventCallback` with tuples:

```razor
<!-- Child component -->
@code {
    [Parameter]
    public Func<ulong, string, string, Task> OnSave { get; set; } = null!;
}

<!-- Parent component -- works naturally -->
<ChildComponent OnSave="SaveAsync" />
```

`Func<..., Task>` works as a Blazor `[Parameter]` and accepts method groups directly. Use `EventCallback<T>` only when `T` is a simple type (not a tuple), or when you need `StateHasChanged` to fire automatically on the parent.

### EventCallback.Factory.Create — also fails with method groups

`EventCallback.Factory.Create<(ulong, string, string)>(this, SaveAsync)` does **not** compile either — same error. The root cause is the same: Blazor's code generator cannot reconcile the method group with a tuple-typed EventCallback.

### Admin-only UI gating

To show/hide content based on user role (without a separate `[Authorize(Roles="admin")]` page attribute), read the role claim in `OnInitializedAsync` and gate with `@if`:

```razor
@page "/profil"
@attribute [Authorize]
@inject AuthenticationStateProvider AuthStateProvider

@code {
    private bool _isAdmin;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var roleClaim = authState.User.FindFirst(ClaimTypes.Role);
        _isAdmin = roleClaim?.Value == "admin";
    }
}

<!-- In markup -->
@if (_isAdmin)
{
    <MudButton Href="/admin-dashboard" Color="Color.Warning">Admin Dashboard</MudButton>
}
```

This works on shared pages (like profile) where both regular users and admins land, but only admins see the admin controls.

### Reusable sub-component pattern for tabbed content

When you have repeated tab structures (e.g., 3 types × 3 languages), extract a sub-component that receives the data list and a save callback:

```razor
<!-- LanguageTabs.razor -->
@code {
    [Parameter] public List<EmailTemplate> Templates { get; set; } = new();
    [Parameter] public Func<ulong, string, string, Task> OnSave { get; set; } = null!;
}
```

Usage in parent:
```razor
<MudTabPanel Text="Inscription">
    <LanguageTabs Templates="@_confirmTemplates" OnSave="SaveAsync" />
</MudTabPanel>
```

The sub-component manages its own internal state (which language tab is active, editor values) while the parent handles the actual save to the database.
