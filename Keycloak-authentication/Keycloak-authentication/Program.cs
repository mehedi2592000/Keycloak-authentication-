using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "Keycloak"; // Keycloak is used for external authentication challenges
})
    .AddCookie()
    .AddOpenIdConnect("Keycloak", options =>
    {
        options.Authority = builder.Configuration["Authentication:Keycloak:Authority"];
        options.ClientId = builder.Configuration["Authentication:Keycloak:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Keycloak:ClientSecret"];
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.CallbackPath = builder.Configuration["Authentication:Keycloak:CallbackPath"];
        options.RequireHttpsMetadata = false; // Development mode allows HTTP
    });

// Add services to the container.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.None; // Important to avoid cookie issues
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Enforce HTTPS
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Enable authentication
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
