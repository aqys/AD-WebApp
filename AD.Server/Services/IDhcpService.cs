using AD.Server.Models;

public interface IDhcpService
{
    List<DhcpLeaseDto> GetAllLeases();
    List<DhcpScopeDto> GetAllScopes();
}
