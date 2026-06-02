using AD.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AD.Server.Controllers
{
    [Authorize]
    public class HomeController(ILogger<HomeController> logger) : Controller
    {
        public IActionResult Index()
        {
            var retval = new HomeViewModel
            {
                Name = HttpContext.User?.Identity?.Name ?? "",
                Shortcuts = new List<HQPanelEntry>(),
            };

            var allClaims = HttpContext.User?.Claims.ToList() ?? new List<Claim>();
            retval.Claims = allClaims
                .Select(c => new ClaimInfo { Type = c.Type, Value = c.Value })
                .ToList();

            var roleClaimTypes = new[]
            {
                ClaimTypes.Role,
                "role",
                "roles",
                "group",
                "groups",
                "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                "http://schemas.microsoft.com/ws/2008/06/identity/claims/groups"
            };

            var roleNames = allClaims
                .Where(c => roleClaimTypes.Contains(c.Type, StringComparer.OrdinalIgnoreCase))
                .Select(c => c.Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            retval.Roles = roleNames;

            if (allClaims.Any(c => string.Equals(c.Type, "groupnumber", StringComparison.OrdinalIgnoreCase)))
            {
                retval.GroupNumber = allClaims.FirstOrDefault(c => string.Equals(c.Type, "groupnumber", StringComparison.OrdinalIgnoreCase))?.Value;
            }

            var isAdmin = roleNames.Any(r => string.Equals(r, "Domain Admins", StringComparison.OrdinalIgnoreCase))
                || roleNames.Any(r => string.Equals(r, "Administrators", StringComparison.OrdinalIgnoreCase));

            if (isAdmin)
            {
                retval.Shortcuts.AddRange(new List<HQPanelEntry>
                {
                    new() { Icon = "fa-solid fa-user", Title = "Brugerstyring", URL = "./User" },
                    new() { Icon = "fa-solid fa-users", Title = "Gruppestyring", URL = "./Group" },
                });
            }
            else
            {
                logger.LogDebug("User {Username} is not admin. Groups: {Groups}",
                    HttpContext.User?.Identity?.Name,
                    string.Join(", ", roleNames));
            }

            return View(retval);
        }
    }
}
