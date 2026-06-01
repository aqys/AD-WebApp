using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(options =>
{
    options.Authority = builder.Configuration["Authentication:Adfs:Authority"];
    options.ClientId = builder.Configuration["Authentication:Adfs:ClientId"];
    options.CallbackPath = builder.Configuration["Authentication:Adfs:CallbackPath"];
    options.SaveTokens = true;
    options.ClientSecret = builder.Configuration["Authentication:Adfs:ClientSecret"];
    options.ResponseType = "code";
    
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        NameClaimType = "name",
        RoleClaimType = "roles"
    };
    // Allow extra clock skew in development to tolerate server/client time differences.
    // Reduce or remove in production; prefer fixing system time sync instead.
    options.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(120);
    
    options.RequireHttpsMetadata = false;
    options.BackchannelHttpHandler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    };
});

builder.Services.AddScoped<IActiveDirectoryService, ActiveDirectoryService>();

builder.Services.AddControllers();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("Domain Admins"));
});

var app = builder.Build();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "auth successful");

app.Run();