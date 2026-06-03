namespace AD.Server.Models;

public class SecurityGroupDto
{
    public string Name { get; set; } = string.Empty;
    public string DistinguishedName { get; set; } = string.Empty;
}

public class SecurityGroupCreateDto
{
    public string GroupName { get; set; } = string.Empty;
    public string ParentPath { get; set; } = string.Empty;
}

public class SecurityGroupDeleteDto
{
    public string DistinguishedName { get; set; } = string.Empty;
}

public class SecurityGroupMemberDto
{
    public string GroupDn { get; set; } = string.Empty;
    public string UserDn { get; set; } = string.Empty;
}
