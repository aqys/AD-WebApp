using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;


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

    options.Scope.Add("openid");
    options.Scope.Add("allatclaims");
    options.GetClaimsFromUserInfoEndpoint = false;

    
    options.ClaimActions.MapJsonKey("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "role");
    options.ClaimActions.MapJsonKey("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "roles");
    
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    };
    options.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(120);
    
    options.RequireHttpsMetadata = false;
    options.BackchannelHttpHandler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
    };
    
    options.Events = new OpenIdConnectEvents
    {
        OnRedirectToIdentityProvider = context =>
        {
            if (context.ProtocolMessage.RequestType == Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectRequestType.Authentication)
            {
                context.ProtocolMessage.SetParameter("resource", builder.Configuration["Authentication:Adfs:ApiIdentifier"]);
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var accessToken = context.TokenEndpointResponse?.AccessToken;
            if (!string.IsNullOrEmpty(accessToken))
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                if (handler.CanReadToken(accessToken))
                {
                    var token = handler.ReadJwtToken(accessToken);
                    var identity = (System.Security.Claims.ClaimsIdentity)context.Principal.Identity;
                    foreach (var claim in token.Claims)
                    {
                        if (!identity.HasClaim(c => c.Type == claim.Type && c.Value == claim.Value))
                        {
                            identity.AddClaim(claim);
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddScoped<IActiveDirectoryService, ActiveDirectoryService>();

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("Domain Admins"));
});

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();