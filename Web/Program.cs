using Microsoft.EntityFrameworkCore;
using Web.Data;
using AppAuth;
using LanguagePack;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Localization;
using Web.Components;
using Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using AppConfig;
using DB.Core;

var builder = WebApplication.CreateBuilder(args);

var store = new LanguageStore(Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "System", "language.db"));
var Keys = new KeysStore(Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "System", "Config.db"));

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddSingleton(store);
builder.Services.AddSingleton(Keys);

builder.Services.AddScoped<ConfigKeys>();
builder.Services.AddScoped<GlobalService>();

builder.Services.AddScoped<Translator>();
builder.Services.AddScoped<UserAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<UserAuthStateProvider>());
builder.Services.AddScoped<IUserAuthService, UserAuthService>();
builder.Services.AddScoped<DBContextClass>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();

    string connectionString = configuration.GetConnectionString("SQLConnection")
        ?? throw new InvalidOperationException("SQLConnection is missing.");

    return new DBContextClass(connectionString);
});

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";
        options.Cookie.Name = "AssetsInspectorCookie";
    });

// =========================================================
// YEH NAYI LINES ADD KI HAIN APNE DATABASE KE LIYE
// =========================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=translations.db";
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(connectionString));
// =========================================================

var app = builder.Build();

#region Language

var codes = store.Codes;

app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture(codes[0])
    .AddSupportedCultures(codes)
    .AddSupportedUICultures(codes));

app.MapGet("/culture/set", (string culture, string redirectUri, HttpContext http) =>
{
    if (codes.Contains(culture))
    {
        http.Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true
            });
    }
    return Results.LocalRedirect(redirectUri);
}).AllowAnonymous();

#endregion

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AllowAnonymous();

app.Run();