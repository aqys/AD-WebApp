using AD.Server.Models;

public interface IActiveDirectoryService
{
    bool CreateUser(UserCreateDto userDto);
    bool DisableUser(string samAccountName);
    bool ChangePassword(string samAccountName, string newPassword);
    bool ChangeUsername(string oldUsername, string newUsername);
    bool CreateGroup(string groupName, string? description);
    bool CreateOU(string ouName, string parentPath);
    bool AssignUserToOU(string username, string ouPath);
    bool RemoveUserFromOU(string username);
}