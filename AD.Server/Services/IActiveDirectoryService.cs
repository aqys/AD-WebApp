using AD.Server.Models;

public interface IActiveDirectoryService
{
    // User management
    List<AdUserDto> GetAllUsers();
    bool CreateUser(UserCreateDto userDto);
    bool DisableUser(string samAccountName);
    bool ChangePassword(string samAccountName, string newPassword);
    bool ChangeDisplayName(string samAccountName, string firstName, string lastName);

    // OU management
    List<string> GetAllOUs();
    bool CreateOU(string ouName, string parentPath);
    bool AssignUserToOU(string username, string ouPath);
    bool RemoveUserFromOU(string username);
}