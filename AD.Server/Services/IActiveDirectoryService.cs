using AD.Server.Models;

public interface IActiveDirectoryService
{
    
    List<AdUserDto> GetAllUsers();
    bool CreateUser(UserCreateDto userDto);
    bool DisableUser(string samAccountName);
    bool EnableUser(string samAccountName);
    bool ChangePassword(string samAccountName, string newPassword);
    bool ChangeDisplayName(string samAccountName, string firstName, string lastName);

    
    List<string> GetAllOUs();
    bool CreateOU(string ouName, string parentPath);
    bool AssignUserToOU(string username, string ouPath);
    bool RemoveUserFromOU(string username);

    
    List<SecurityGroupDto> GetAllSecurityGroups();
    bool CreateSecurityGroup(string groupName, string parentPath);
    bool DeleteSecurityGroup(string distinguishedName);
    List<string> GetGroupMembers(string distinguishedName);
    bool AddMemberToGroup(string groupDn, string userDn);
    bool RemoveMemberFromGroup(string groupDn, string userDn);
}