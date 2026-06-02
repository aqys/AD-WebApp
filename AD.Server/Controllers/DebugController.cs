using Microsoft.AspNetCore.Mvc;
using AD.Server.Models;
using Novell.Directory.Ldap;

/// <summary>
/// Temporary debug controller — remove before production deployment.
/// Bypasses auth to isolate whether the issue is authentication or AD connectivity.
/// </summary>
[ApiController]
[Route("api/debug")]
public class DebugController : ControllerBase
{
    private readonly IActiveDirectoryService _adService;
    private readonly IWebHostEnvironment _env;

    public DebugController(IActiveDirectoryService adService, IWebHostEnvironment env)
    {
        _adService = adService;
        _env = env;
    }

    // GET /api/debug/users — list users (dev only, no auth)
    [HttpGet("users")]
    public IActionResult GetUsers()
    {
        if (!_env.IsDevelopment())
            return NotFound();

        // Call the service directly and catch here so we see the real error
        try
        {
            var users = _adService.GetAllUsers();
            return Ok(new { count = users.Count, users });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message, type = ex.GetType().Name, stack = ex.ToString() });
        }
    }

    // GET /api/debug/ous — list OUs (dev only, no auth)
    [HttpGet("ous")]
    public IActionResult GetOUs()
    {
        if (!_env.IsDevelopment())
            return NotFound();

        try
        {
            var ous = _adService.GetAllOUs();
            return Ok(new { count = ous.Count, ous });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message, type = ex.GetType().Name, stack = ex.ToString() });
        }
    }

    // GET /api/debug/ldap — test raw LDAP connection
    [HttpGet("ldap")]
    public IActionResult TestLdap([FromServices] IConfiguration config)
    {
        if (!_env.IsDevelopment())
            return NotFound();

        var server = config["ActiveDirectory:LdapServer"] ?? config["ActiveDirectory:Domain"];
        var username = config["ActiveDirectory:Username"];
        var password = config["ActiveDirectory:Password"];
        var hasPassword = !string.IsNullOrEmpty(password);

        try
        {
            using var connection = new LdapConnection();
            connection.SecureSocketLayer = false;
            connection.UserDefinedServerCertValidationDelegate += (sender, cert, chain, errors) => true;
            
            connection.Connect(server, 389);
            connection.StartTls();
            
            if (!string.IsNullOrEmpty(username))
            {
                string bindUsername = username;
                if (username.Contains("\\"))
                {
                    var parts = username.Split('\\');
                    bindUsername = $"{parts[1]}@{parts[0]}.local".ToLower();
                }
                connection.Bind(bindUsername, password);
            }
            else
            {
                connection.Bind(null, null);
            }

            return Ok(new
            {
                connected = connection.Bound,
                server = server,
                usingCredentials = !string.IsNullOrEmpty(username),
                hasPassword,
                username
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                connected = false,
                server = server,
                error = ex.Message,
                type = ex.GetType().Name,
                usingCredentials = !string.IsNullOrEmpty(username),
                hasPassword,
                username
            });
        }
    }

    // GET /api/debug/auth — show current auth status and claims
    [HttpGet("auth")]
    public IActionResult GetAuth()
    {
        if (!_env.IsDevelopment())
            return NotFound();

        return Ok(new
        {
            isAuthenticated = User.Identity?.IsAuthenticated ?? false,
            name = User.Identity?.Name,
            claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
        });
    }
}
