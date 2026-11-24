using Web.Components;
using Microsoft.AspNetCore.Localization;
using LanguagePack;
using Web.Services;
using DB.Core;

var builder = WebApplication.CreateBuilder(args);

var store = new LanguageStore(Path.Combine(builder.Environment.ContentRootPath, "Data", "language.db"));

builder.Services.AddSingleton(store);
builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
builder.Services.AddSingleton<GlobalService>();

builder.Services.AddScoped<Translator>();
builder.Services.AddScoped<IDatabaseService, DatabaseService>();



builder.Services.AddRazorComponents().AddInteractiveServerComponents();

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
});

#endregion

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);


app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();