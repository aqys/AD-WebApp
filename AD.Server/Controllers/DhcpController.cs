using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/[controller]")]
public class DhcpController : ControllerBase
{
    private readonly IDhcpService _dhcpService;

    public DhcpController(IDhcpService dhcpService)
    {
        _dhcpService = dhcpService;
    }

    [HttpGet("leases")]
    public IActionResult GetLeases()
    {
        var leases = _dhcpService.GetAllLeases();
        return Ok(leases);
    }

    [HttpGet("scopes")]
    public IActionResult GetScopes()
    {
        var scopes = _dhcpService.GetAllScopes();
        return Ok(scopes);
    }
}
