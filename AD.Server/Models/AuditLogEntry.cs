using System;

namespace AD.Server.Models;

public class AuditLogEntry
{
    public DateTime Timestamp { get; set; }
    public string Administrator { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
}
