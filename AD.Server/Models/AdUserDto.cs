namespace AD.Server.Models;

public class AdUserDto
{
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DistinguishedName { get; set; } = string.Empty;
    public string? OUPath { get; set; }
    public bool IsEnabled { get; set; }
}
