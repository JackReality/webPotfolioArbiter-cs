using DotNetEnv;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Portfolio.Components;
using Portfolio.Data;
using Portfolio.Services;
using System.Globalization;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// --- Chargement du fichier .env (identifiants MySQL) -----------------------
Env.Load();   // lit le .env à la racine et le met dans les variables d'environnement

var connectionString =
    $"server={Env.GetString("DB_HOST")};" +
    $"port={Env.GetString("DB_PORT")};" +
    $"database={Env.GetString("DB_NAME")};" +
    $"user={Env.GetString("DB_USER")};" +
    $"password={Env.GetString("DB_PASSWORD")}";

// --- Composants Blazor (rendu serveur) -------------------------------------
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// --- MudBlazor -------------------------------------------------------------
builder.Services.AddMudServices();

// --- Multilingue (FR / EN / ES) --------------------------------------------
// AddLocalization rend disponible IStringLocalizer<T> ; "Resources" est le
// dossier où vivent les fichiers .resx (les traductions).
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var languesSupportees = new[] { "fr", "en", "es" };   // français par défaut

// --- Base de données MySQL (EF Core + Pomelo) ------------------------------
// On fixe la version du serveur manuellement (MySqlServerVersion) plutôt que
// AutoDetect, pour que l'app démarre même si la base n'est pas encore lancée.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0))));

// --- Authentification (cookie natif) ---------------------------------------
// Mécanisme natif de .NET : après login, un cookie chiffré contient l'identité
// (id, email, rôle). À chaque requête, .NET le lit et reconstruit "User".
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";              // pas connecté → page de connexion
        options.AccessDeniedPath = "/acces-reserve"; // connecté mais pas le bon rôle
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();   // rend l'identité dispo dans toutes les pages

// --- Services métier (injectés dans les pages) -----------------------------
builder.Services.AddScoped<ExempleService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();

var app = builder.Build();

// --- Multilingue : applique la langue choisie (lue dans un cookie) ---------
// À chaque requête, ce middleware fixe CultureInfo.CurrentUICulture selon le
// cookie de langue. Doit être placé TÔT, avant le rendu des composants.
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture("fr")
    .AddSupportedCultures(languesSupportees)
    .AddSupportedUICultures(languesSupportees));

// --- Pipeline HTTP ---------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // Valeur HSTS par défaut : 30 jours. À ajuster pour la production.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// L'ordre compte : authentification (qui es-tu ?) puis autorisation (as-tu le droit ?).
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();

// --- Endpoint de changement de langue --------------------------------------
// Le sélecteur FR|EN|ES du header pointe ici. On enregistre la langue dans un
// cookie, puis on RECHARGE la page (LocalRedirect) pour que toute l'app passe
// dans la nouvelle langue. Un rechargement complet est nécessaire en Blazor
// Server pour que le circuit reparte dans la bonne culture.
app.MapGet("/Culture/Set", (string culture, string redirectUri, HttpContext ctx) =>
{
    if (languesSupportees.Contains(culture))
    {
        ctx.Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(
                new RequestCulture(culture, culture)));
    }
    return Results.LocalRedirect(redirectUri);
});

// --- Endpoints de connexion / déconnexion ----------------------------------
// La page /login poste vers ici. On vérifie le mot de passe, puis on POSE le
// cookie d'identité (SignInAsync) avec le rôle en "claim". Besoin du HttpContext
// → c'est pour ça que c'est un endpoint et pas un bouton interactif.
//VerifierIdentifiantsAsync() est écrite dans AuthService.cs
app.MapPost("/auth/login", async (
    HttpContext ctx, AuthService auth,
    [FromForm] string email, [FromForm] string password, [FromForm] string? returnUrl) =>
{
    var user = await auth.VerifierIdentifiantsAsync(email ?? "", password ?? "");
    if (user is null)
        return Results.LocalRedirect($"/login?error=1&returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");

    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Name, user.DisplayName),
        new(ClaimTypes.Email, user.Email),
        new(ClaimTypes.Role, user.Role),   // <- pilote [Authorize(Roles=...)]
    };
    var principal = new ClaimsPrincipal(
        new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    return Results.LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
});

app.MapPost("/auth/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.LocalRedirect("/");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
