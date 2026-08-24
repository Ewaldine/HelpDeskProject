using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// HttpClient to call HelpDesk.API
builder.Services.AddHttpClient("HelpDeskApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
});
builder.Services.AddScoped<HelpDesk.Web.Services.ITicketApiService, HelpDesk.Web.Services.TicketApiService>();
builder.Services.AddHttpContextAccessor();

// Authentication — Keycloak via OpenID Connect
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
})
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Authority = builder.Configuration["Keycloak:Authority"];
    options.ClientId = builder.Configuration["Keycloak:ClientId"];
    options.ClientSecret = builder.Configuration["Keycloak:ClientSecret"];
    options.ResponseType = "code";
    options.RequireHttpsMetadata = false;
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.CorrelationCookie.SameSite = SameSiteMode.Lax;
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");

    options.TokenValidationParameters.NameClaimType = "preferred_username";
    options.TokenValidationParameters.RoleClaimType = System.Security.Claims.ClaimTypes.Role;

    options.Events = new Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectEvents
    {
        OnTokenValidated = context =>
        {
            var identity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
            if (identity == null) return Task.CompletedTask;

            var accessToken = context.TokenEndpointResponse?.AccessToken;
            if (!string.IsNullOrEmpty(accessToken))
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(accessToken);

                var realmAccessClaim = jwt.Claims.FirstOrDefault(c => c.Type == "realm_access");
                if (realmAccessClaim != null)
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(realmAccessClaim.Value);
                    if (doc.RootElement.TryGetProperty("roles", out var rolesElement))
                    {
                        foreach (var role in rolesElement.EnumerateArray())
                        {
                            var roleValue = role.GetString();
                            if (!string.IsNullOrEmpty(roleValue))
                            {
                                identity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, roleValue));
                            }
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
