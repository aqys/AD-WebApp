using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using AD.Server.Models;
using Microsoft.AspNetCore.Hosting;

public class AuditLogService : IAuditLogService
{
    private readonly string _logFilePath;
    private static readonly object _fileLock = new();

    public AuditLogService(IWebHostEnvironment env)
    {
        var dataDir = Path.Combine(env.ContentRootPath, "Data");
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }
        _logFilePath = Path.Combine(dataDir, "audit_logs.json");
    }

    public void LogAction(string administrator, string action, string target, string details, string ipAddress)
    {
        var entry = new AuditLogEntry
        {
            Timestamp = DateTime.UtcNow,
            Administrator = administrator,
            Action = action,
            Target = target,
            Details = details,
            IpAddress = ipAddress
        };

        lock (_fileLock)
        {
            List<AuditLogEntry> logs;
            try
            {
                if (File.Exists(_logFilePath))
                {
                    var json = File.ReadAllText(_logFilePath);
                    logs = JsonSerializer.Deserialize<List<AuditLogEntry>>(json) ?? new List<AuditLogEntry>();
                }
                else
                {
                    logs = new List<AuditLogEntry>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuditLogService] Reading existing logs failed, starting fresh: {ex.Message}");
                logs = new List<AuditLogEntry>();
            }

            logs.Add(entry);

            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(logs, options);
                File.WriteAllText(_logFilePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuditLogService] Error writing log: {ex}");
            }
        }
    }

    public List<AuditLogEntry> GetAuditLogs()
    {
        lock (_fileLock)
        {
            try
            {
                if (!File.Exists(_logFilePath))
                {
                    return new List<AuditLogEntry>();
                }
                var json = File.ReadAllText(_logFilePath);
                return JsonSerializer.Deserialize<List<AuditLogEntry>>(json) ?? new List<AuditLogEntry>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuditLogService] Error reading logs: {ex}");
                return new List<AuditLogEntry>();
            }
        }
    }
}
