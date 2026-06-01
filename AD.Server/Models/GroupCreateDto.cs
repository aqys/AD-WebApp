namespace AD.Server.Models;

public class GroupCreateDto
{
    public required string GroupName { get; set; }
    public string? Description { get; set; }
}
