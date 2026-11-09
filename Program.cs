using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using POE_RaceDay_MVC.Filters;
using POE_RaceDay_MVC.Services;

var builder = WebApplication.CreateBuilder(args);

string apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
    ?? throw new InvalidOperationException(
        "ApiSettings:BaseUrl is missing from configuration.");

builder.Services.AddControllersWithViews(options =>
{
    // Only fields marked [Required] are required (not every non-nullable
    // property of a view model).
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;

    // Every POST is protected against CSRF.
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());

    // Safety net: API failures become friendly messages, never stack traces.
    options.Filters.Add<ApiExceptionFilter>();
});

// Session state keeps the user's login (JWT) and role across pages.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.Name = ".RaceDay.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ISessionUser, SessionUser>();

// The one and only data source: the Part 2 API.
builder.Services.AddHttpClient<IRaceDayApi, RaceDayApi>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

// Friendly error pages in every environment (no raw errors for users).
app.UseExceptionHandler("/Home/Error");
app.UseStatusCodePagesWithReExecute("/Home/Error", "?code={0}");

// One fixed culture so decimals ("5.5") and dates behave identically on a
// developer machine and inside the Docker container.
var culture = new CultureInfo("en-US");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(culture),
    SupportedCultures = new List<CultureInfo> { culture },
    SupportedUICultures = new List<CultureInfo> { culture }
});

app.UseStaticFiles();

// Authenticated pages must never be served from the browser cache, so the
// back button after logging out cannot reveal protected content.
app.Use(async (context, next) =>
{
    context.Response.Headers.CacheControl = "no-store";
    await next();
});

app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
