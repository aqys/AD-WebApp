namespace AD.Server.Models;

public class ChangeDisplayNameDto
{
    public required string Username { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
}
