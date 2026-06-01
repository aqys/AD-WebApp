namespace AD.Server.Models;

public class UsernameChangeDto
{
    public required string OldUsername { get; set; }
    public required string NewUsername { get; set; }
}
