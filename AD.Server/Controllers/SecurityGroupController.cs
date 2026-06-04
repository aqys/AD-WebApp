using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AD.Server.Models;

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/[controller]")]
public class SecurityGroupController : ControllerBase
{
    private readonly IActiveDirectoryService _adService;
    private readonly IAuditLogService _auditLogService;

    public SecurityGroupController(IActiveDirectoryService adService, IAuditLogService auditLogService)
    {
        _adService = adService;
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public IActionResult GetGroups()
    {
        var groups = _adService.GetAllSecurityGroups();
        return Ok(groups);
    }

    [HttpPost("create")]
    public IActionResult CreateGroup([FromBody] SecurityGroupCreateDto dto)
    {
        var success = _adService.CreateSecurityGroup(dto.GroupName, dto.ParentPath);
        if (success)
        {
            _auditLogService.LogAction(GetAdminUser(), "Sikkerhedsgruppe oprettet", dto.GroupName, $"Oprettede sikkerhedsgruppen: {dto.GroupName} under parent: {dto.ParentPath}", GetIpAddress());
            return Ok(new { message = $"Gruppe '{dto.GroupName}' oprettet." });
        }
        return BadRequest(new { message = "Kunne ikke oprette gruppen." });
    }

    [HttpPost("delete")]
    public IActionResult DeleteGroup([FromBody] SecurityGroupDeleteDto dto)
    {
        var success = _adService.DeleteSecurityGroup(dto.DistinguishedName);
        if (success)
        {
            _auditLogService.LogAction(GetAdminUser(), "Sikkerhedsgruppe slettet", dto.DistinguishedName, $"Slettede sikkerhedsgruppen: {dto.DistinguishedName}", GetIpAddress());
            return Ok(new { message = "Gruppe slettet." });
        }
        return BadRequest(new { message = "Kunne ikke slette gruppen." });
    }

    [HttpGet("members")]
    public IActionResult GetMembers([FromQuery] string groupDn)
    {
        var members = _adService.GetGroupMembers(groupDn);
        return Ok(members);
    }

    [HttpPost("add-member")]
    public IActionResult AddMember([FromBody] SecurityGroupMemberDto dto)
    {
        var success = _adService.AddMemberToGroup(dto.GroupDn, dto.UserDn);
        if (success)
        {
            _auditLogService.LogAction(GetAdminUser(), "Medlem tilføjet gruppe", dto.GroupDn, $"Tilføjede bruger {dto.UserDn} til gruppen {dto.GroupDn}", GetIpAddress());
            return Ok(new { message = "Bruger tilføjet til gruppen." });
        }
        return BadRequest(new { message = "Kunne ikke tilføje bruger." });
    }

    [HttpPost("remove-member")]
    public IActionResult RemoveMember([FromBody] SecurityGroupMemberDto dto)
    {
        var success = _adService.RemoveMemberFromGroup(dto.GroupDn, dto.UserDn);
        if (success)
        {
            _auditLogService.LogAction(GetAdminUser(), "Medlem fjernet fra gruppe", dto.GroupDn, $"Fjernede bruger {dto.UserDn} fra gruppen {dto.GroupDn}", GetIpAddress());
            return Ok(new { message = "Bruger fjernet fra gruppen." });
        }
        return BadRequest(new { message = "Kunne ikke fjerne bruger." });
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
