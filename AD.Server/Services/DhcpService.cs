using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using AD.Server.Models;
using Microsoft.Extensions.Configuration;

public class DhcpService : IDhcpService
{
    private readonly IConfiguration _config;

    private static readonly Dictionary<string, string> ScopeVlanMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "192.168.10", "VLAN 10 - Klienter" },
        { "192.168.30", "VLAN 30 - Wi-Fi" },
        { "192.168.20", "VLAN 20 - Servere" },
        { "192.168.99", "VLAN 99 - Management" },
    };

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public DhcpService(IConfiguration config)
    {
        _config = config;
    }

    private string[] GetRelayUrls()
    {
        var urls = _config["Dhcp:RelayUrls"];
        if (string.IsNullOrWhiteSpace(urls))
            return Array.Empty<string>();
        return urls.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private HttpClient CreateClient()
    {
        var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(15);
        var apiKey = _config["Dhcp:ApiKey"];
        if (!string.IsNullOrWhiteSpace(apiKey))
            client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
        return client;
    }

    private string ResolveVlan(string scopeId)
    {
        foreach (var kv in ScopeVlanMap)
        {
            if (scopeId.StartsWith(kv.Key, StringComparison.OrdinalIgnoreCase))
                return kv.Value;
        }
        return "Ukendt VLAN";
    }

    public List<DhcpLeaseDto> GetAllLeases()
    {
        return GetAllLeasesAsync().GetAwaiter().GetResult();
    }

    public List<DhcpScopeDto> GetAllScopes()
    {
        return GetAllScopesAsync().GetAwaiter().GetResult();
    }

    private async Task<List<DhcpLeaseDto>> GetAllLeasesAsync()
    {
        var result = new List<DhcpLeaseDto>();
        var relayUrls = GetRelayUrls();

        foreach (var baseUrl in relayUrls)
        {
            try
            {
                using var client = CreateClient();
                var json = await client.GetStringAsync($"{baseUrl.TrimEnd('/')}/leases");

                if (string.IsNullOrWhiteSpace(json) || json.Trim() == "[]")
                    continue;

                var items = json.Trim().StartsWith('[')
                    ? JsonSerializer.Deserialize<List<RelayLeaseItem>>(json, JsonOpts)
                    : new List<RelayLeaseItem> { JsonSerializer.Deserialize<RelayLeaseItem>(json, JsonOpts)! };

                if (items == null) continue;

                var serverHost = new Uri(baseUrl).Host;

                foreach (var item in items)
                {
                    result.Add(new DhcpLeaseDto
                    {
                        IpAddress = item.IpAddress ?? "",
                        HostName = item.HostName ?? "",
                        MacAddress = item.MacAddress ?? "",
                        LeaseExpires = item.LeaseExpires ?? "",
                        ScopeId = item.ScopeId ?? "",
                        Vlan = ResolveVlan(item.ScopeId ?? ""),
                        Server = serverHost
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DhcpService] GetAllLeases error for '{baseUrl}': {ex.Message}");
            }
        }

        return result.OrderBy(l => l.Vlan).ThenBy(l => l.IpAddress).ToList();
    }

    private async Task<List<DhcpScopeDto>> GetAllScopesAsync()
    {
        var result = new List<DhcpScopeDto>();
        var relayUrls = GetRelayUrls();

        foreach (var baseUrl in relayUrls)
        {
            try
            {
                using var client = CreateClient();
                var json = await client.GetStringAsync($"{baseUrl.TrimEnd('/')}/scopes");

                if (string.IsNullOrWhiteSpace(json) || json.Trim() == "[]")
                    continue;

                var items = json.Trim().StartsWith('[')
                    ? JsonSerializer.Deserialize<List<RelayScopeItem>>(json, JsonOpts)
                    : new List<RelayScopeItem> { JsonSerializer.Deserialize<RelayScopeItem>(json, JsonOpts)! };

                if (items == null) continue;

                var serverHost = new Uri(baseUrl).Host;

                foreach (var item in items)
                {
                    result.Add(new DhcpScopeDto
                    {
                        ScopeId = item.ScopeId ?? "",
                        Name = item.Name ?? "",
                        SubnetMask = item.SubnetMask ?? "",
                        StartRange = item.StartRange ?? "",
                        EndRange = item.EndRange ?? "",
                        State = item.State ?? "",
                        Vlan = ResolveVlan(item.ScopeId ?? ""),
                        Server = serverHost,
                        ActiveLeases = item.ActiveLeases
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DhcpService] GetAllScopes error for '{baseUrl}': {ex.Message}");
            }
        }

        return result.OrderBy(s => s.Vlan).ThenBy(s => s.ScopeId).ToList();
    }

    private sealed class RelayLeaseItem
    {
        public string? IpAddress { get; set; }
        public string? HostName { get; set; }
        public string? MacAddress { get; set; }
        public string? LeaseExpires { get; set; }
        public string? ScopeId { get; set; }
    }

    private sealed class RelayScopeItem
    {
        public string? ScopeId { get; set; }
        public string? Name { get; set; }
        public string? SubnetMask { get; set; }
        public string? StartRange { get; set; }
        public string? EndRange { get; set; }
        public string? State { get; set; }
        public int ActiveLeases { get; set; }
    }
}
