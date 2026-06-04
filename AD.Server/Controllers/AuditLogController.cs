using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AD.Server.Models;

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/[controller]")]
public class AuditLogController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public IActionResult GetAuditLogs()
    {
        var logs = _auditLogService.GetAuditLogs();
        var orderedLogs = logs.OrderByDescending(l => l.Timestamp).ToList();
        return Ok(orderedLogs);
    }
}
