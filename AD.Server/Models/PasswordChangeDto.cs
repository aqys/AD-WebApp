namespace AD.Server.Models;

public class PasswordChangeDto
{
    public required string Username { get; set; }
    public required string NewPassword { get; set; }
}
