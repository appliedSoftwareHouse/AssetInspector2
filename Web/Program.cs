using AppAuth;
using LanguagePack;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Localization;
using Web.Components;
using Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

var store = new LanguageStore(Path.Combine(builder.Environment.ContentRootPath, "System", "language.db"));
builder.Services.AddSingleton(store);

builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
builder.Services.AddSingleton<GlobalService>();

builder.Services.AddScoped<Translator>();
builder.Services.AddScoped<IDatabaseService, DatabaseService>();

builder.Services.AddScoped<UserAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>sp.GetRequiredService<UserAuthStateProvider>());
builder.Services.AddScoped<IUserAuthService, UserAuthService>();


builder.Services.AddRazorComponents().AddInteractiveServerComponents();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login"; // Adjust to your login page route
        options.AccessDeniedPath = "/access-denied"; // Optional
        options.Cookie.Name = "AssetsInspectorCookie";

    });



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