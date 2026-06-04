using System.Collections.Generic;
using AD.Server.Models;

public interface IAuditLogService
{
    void LogAction(string administrator, string action, string target, string details, string ipAddress);
    List<AuditLogEntry> GetAuditLogs();
}
