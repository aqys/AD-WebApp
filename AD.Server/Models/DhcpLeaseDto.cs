namespace AD.Server.Models;

public class DhcpLeaseDto
{
    public string IpAddress { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public string MacAddress { get; set; } = string.Empty;
    public string LeaseExpires { get; set; } = string.Empty;
    public string ScopeId { get; set; } = string.Empty;
    public string Vlan { get; set; } = string.Empty;
    public string Server { get; set; } = string.Empty;
}

public class DhcpScopeDto
{
    public string ScopeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SubnetMask { get; set; } = string.Empty;
    public string StartRange { get; set; } = string.Empty;
    public string EndRange { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Vlan { get; set; } = string.Empty;
    public string Server { get; set; } = string.Empty;
    public int ActiveLeases { get; set; }
}
