using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AD.Server.Models;

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IActiveDirectoryService _adService;

    public UserController(IActiveDirectoryService adService)
    {
        _adService = adService;
    }

    [HttpPost("create")]
    public IActionResult CreateUser([FromBody] UserCreateDto dto)
    {
        var success = _adService.CreateUser(dto);
        if (success) return Ok(new { message = "Bruger oprettet succesfuldt!" });
        return BadRequest(new { message = "Kunne ikke oprette bruger." });
    }

    [HttpPost("disable/{username}")]
    public IActionResult DisableUser(string username)
    {
        var success = _adService.DisableUser(username);
        if (success) return Ok(new { message = $"Bruger {username} er deaktiveret." });
        return NotFound(new { message = "Bruger ikke fundet." });
    }

    [HttpPost("change-password")]
    public IActionResult ChangePassword([FromBody] PasswordChangeDto dto)
    {
        var success = _adService.ChangePassword(dto.Username, dto.NewPassword);
        if (success) return Ok(new { message = "Adgangskode ændret succesfuldt!" });
        return BadRequest(new { message = "Kunne ikke ændre adgangskode." });
    }

    [HttpPost("change-username")]
    public IActionResult ChangeUsername([FromBody] UsernameChangeDto dto)
    {
        var success = _adService.ChangeUsername(dto.OldUsername, dto.NewUsername);
        if (success) return Ok(new { message = "Brugernavn ændret succesfuldt!" });
        return BadRequest(new { message = "Kunne ikke ændre brugernavn." });
    }

    [HttpPost("create-group")]
    public IActionResult CreateGroup([FromBody] GroupCreateDto dto)
    {
        var success = _adService.CreateGroup(dto.GroupName, dto.Description);
        if (success) return Ok(new { message = $"Gruppe {dto.GroupName} oprettet succesfuldt!" });
        return BadRequest(new { message = "Kunne ikke oprette gruppe." });
    }

    [HttpPost("create-ou")]
    public IActionResult CreateOU([FromBody] OUCreateDto dto)
    {
        var success = _adService.CreateOU(dto.OUName, dto.ParentPath);
        if (success) return Ok(new { message = $"OU {dto.OUName} oprettet succesfuldt!" });
        return BadRequest(new { message = "Kunne ikke oprette OU." });
    }

    [HttpPost("assign-ou")]
    public IActionResult AssignUserToOU([FromBody] UserOUDto dto)
    {
        var success = _adService.AssignUserToOU(dto.Username, dto.OUPath);
        if (success) return Ok(new { message = $"Bruger {dto.Username} tildelt til OU succesfuldt!" });
        return BadRequest(new { message = "Kunne ikke tildele bruger til OU." });
    }

    [HttpPost("remove-ou/{username}")]
    public IActionResult RemoveUserFromOU(string username)
    {
        var success = _adService.RemoveUserFromOU(username);
        if (success) return Ok(new { message = $"Bruger {username} fjernet fra OU succesfuldt!" });
        return BadRequest(new { message = "Kunne ikke fjerne bruger fra OU." });
    }
}