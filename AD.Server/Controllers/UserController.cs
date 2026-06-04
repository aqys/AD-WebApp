using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AD.Server.Models;

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IActiveDirectoryService _adService;
    private readonly IAuditLogService _auditLogService;

    public UserController(IActiveDirectoryService adService, IAuditLogService auditLogService)
    {
        _adService = adService;
        _auditLogService = auditLogService;
    }

    
    
    
    [HttpGet]
    public IActionResult GetUsers()
    {
        var users = _adService.GetAllUsers();
        return Ok(users);
    }

    
    
    
    [HttpPost("create")]
    public IActionResult CreateUser([FromBody] UserCreateDto dto)
    {
        var success = _adService.CreateUser(dto);
        if (success)
        {
            _auditLogService.LogAction(GetAdminUser(), "Bruger oprettet", dto.Username, $"Oprettede brugeren: {dto.FirstName} {dto.LastName} ({dto.Username})", GetIpAddress());
            return Ok(new { message = "Bruger oprettet succesfuldt!" });
        }
        return BadRequest(new { message = "Kunne ikke oprette bruger." });
    }

    
    
    
    [HttpPost("disable/{username}")]
    public IActionResult DisableUser(string username)
    {
        var success = _adService.DisableUser(username);
        if (success)
        {
            _auditLogService.LogAction(GetAdminUser(), "Bruger deaktiveret", username, $"Deaktiverede brugeren: {username}", GetIpAddress());
            return Ok(new { message = $"Bruger {username} er deaktiveret." });
        }
        return NotFound(new { message = "Bruger ikke fundet." });
    }

    
    
    
    [HttpPost("enable/{username}")]
    public IActionResult EnableUser(string username)
    {
        var success = _adService.EnableUser(username);
        if (success)
        {
            _auditLogService.LogAction(GetAdminUser(), "Bruger aktiveret", username, $"Aktiverede brugeren: {username}", GetIpAddress());
            return Ok(new { message = $"Bruger {username} er aktiveret." });
        }
        return NotFound(new { message = "Bruger ikke fundet." });
    }

    
    
    
    [HttpPost("change-password")]
    public IActionResult ChangePassword([FromBody] PasswordChangeDto dto)
    {
        var success = _adService.ChangePassword(dto.Username, dto.NewPassword);
        if (success)
        {
            _auditLogService.LogAction(GetAdminUser(), "Adgangskode ændret", dto.Username, $"Ændrede adgangskoden for brugeren: {dto.Username}", GetIpAddress());
            return Ok(new { message = "Adgangskode ændret succesfuldt!" });
        }
        return BadRequest(new { message = "Kunne ikke ændre adgangskode." });
    }

    
    
    
    [HttpPost("change-displayname")]
    public IActionResult ChangeDisplayName([FromBody] ChangeDisplayNameDto dto)
    {
        var success = _adService.ChangeDisplayName(dto.Username, dto.FirstName, dto.LastName);
        if (success)
        {
            _auditLogService.LogAction(GetAdminUser(), "Visningsnavn ændret", dto.Username, $"Ændrede visningsnavnet for brugeren {dto.Username} til: {dto.FirstName} {dto.LastName}", GetIpAddress());
            return Ok(new { message = "Navn ændret succesfuldt!" });
        }
        return BadRequest(new { message = "Kunne ikke ændre navn." });
    }

    
    
    
    [HttpGet("ous")]
    public IActionResult GetOUs()
    {
        var ous = _adService.GetAllOUs();
        return Ok(ous);
    }

    
    
    
    [HttpPost("create-ou")]
    public IActionResult CreateOU([FromBody] OUCreateDto dto)
    {
        var success = _adService.CreateOU(dto.OUName, dto.ParentPath);
        if (success)
        {
            _auditLogService.LogAction(GetAdminUser(), "OU oprettet", dto.OUName, $"Oprettede OU: {dto.OUName} under parent: {dto.ParentPath}", GetIpAddress());
            return Ok(new { message = $"OU {dto.OUName} oprettet succesfuldt!" });
        }
        return BadRequest(new { message = "Kunne ikke oprette OU." });
    }

    
    
    
    [HttpPost("assign-ou")]
    public IActionResult AssignUserToOU([FromBody] UserOUDto dto)
    {
        var success = _adService.AssignUserToOU(dto.Username, dto.OUPath);
        if (success)
        {
            _auditLogService.LogAction(GetAdminUser(), "Tildelt OU", dto.Username, $"Flyttede bruger {dto.Username} til OU: {dto.OUPath}", GetIpAddress());
            return Ok(new { message = $"Bruger {dto.Username} tildelt til OU succesfuldt!" });
        }
        return BadRequest(new { message = "Kunne ikke tildele bruger til OU." });
    }

    
    
    
    [HttpPost("remove-ou/{username}")]
    public IActionResult RemoveUserFromOU(string username)
    {
        var success = _adService.RemoveUserFromOU(username);
        if (success)
        {
            _auditLogService.LogAction(GetAdminUser(), "Bruger fjernet fra OU", username, $"Fjernede bruger {username} fra OU (flyttet til standard-container)", GetIpAddress());
            return Ok(new { message = $"Bruger {username} fjernet fra OU succesfuldt!" });
        }
        return BadRequest(new { message = "Kunne ikke fjerne bruger fra OU." });
    }

    private string GetIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private string GetAdminUser()
    {
        return User.Identity?.Name ?? "Unknown Admin";
    }
}