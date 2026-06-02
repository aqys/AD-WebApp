using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using AD.Server.Models;

public class ActiveDirectoryService : IActiveDirectoryService
{
    private readonly IConfiguration _config;

    public ActiveDirectoryService(IConfiguration config)
    {
        _config = config;
    }

    public bool CreateUser(UserCreateDto userDto)
    {
        try
        {
            using (var context = new PrincipalContext(ContextType.Domain, _config["ActiveDirectory:Domain"], _config["ActiveDirectory:ContainerOU"]))
            {
                using (var user = new UserPrincipal(context))
                {
                    user.SamAccountName = userDto.Username;
                    user.GivenName = userDto.FirstName;
                    user.Surname = userDto.LastName;
                    user.EmailAddress = $"{userDto.Username}@{_config["ActiveDirectory:Domain"]}";
                    user.SetPassword(userDto.Password);
                    user.Enabled = true;
                    user.Save();
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fejl ved oprettelse af bruger: {ex.Message}");
            return false;
        }
    }

    public bool DisableUser(string samAccountName)
    {
        using (var context = new PrincipalContext(ContextType.Domain, _config["ActiveDirectory:Domain"]))
        {
            var user = UserPrincipal.FindByIdentity(context, samAccountName);
            if (user != null)
            {
                user.Enabled = false;
                user.Save();
                return true;
            }
        }
        return false;
    }

    public bool ChangePassword(string samAccountName, string newPassword)
    {
        using (var context = new PrincipalContext(ContextType.Domain, _config["ActiveDirectory:Domain"]))
        {
            var user = UserPrincipal.FindByIdentity(context, samAccountName);
            if (user != null)
            {
                user.SetPassword(newPassword);
                user.Save();
                return true;
            }
        }
        return false;
    }

    public bool ChangeUsername(string oldUsername, string newUsername)
    {
        try
        {
            using (var context = new PrincipalContext(ContextType.Domain, _config["ActiveDirectory:Domain"]))
            {
                var user = UserPrincipal.FindByIdentity(context, oldUsername);
                if (user != null)
                {
                    user.SamAccountName = newUsername;
                    user.UserPrincipalName = $"{newUsername}@{_config["ActiveDirectory:Domain"]}";
                    user.Save();
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fejl ved ændring af brugernavn: {ex.Message}");
            return false;
        }
    }

    public bool CreateGroup(string groupName, string? description)
    {
        try
        {
            using (var context = new PrincipalContext(ContextType.Domain, _config["ActiveDirectory:Domain"], _config["ActiveDirectory:ContainerOU"]))
            {
                using (var group = new GroupPrincipal(context))
                {
                    group.SamAccountName = groupName;
                    group.Name = groupName;
                    if (!string.IsNullOrEmpty(description))
                    {
                        group.Description = description;
                    }
                    group.Save();
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fejl ved oprettelse af gruppe: {ex.Message}");
            return false;
        }
    }

    public bool CreateOU(string ouName, string parentPath)
    {
        try
        {
            using (var entry = new DirectoryEntry($"LDAP://{_config["ActiveDirectory:Domain"]}/{parentPath}"))
            {
                using (var ou = entry.Children.Add($"OU={ouName}", "organizationalUnit"))
                {
                    ou.CommitChanges();
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fejl ved oprettelse af OU: {ex.Message}");
            return false;
        }
    }

    public bool AssignUserToOU(string username, string ouPath)
    {
        try
        {
            using (var context = new PrincipalContext(ContextType.Domain, _config["ActiveDirectory:Domain"]))
            {
                var user = UserPrincipal.FindByIdentity(context, username);
                if (user != null)
                {
                    var userEntry = (DirectoryEntry)user.GetUnderlyingObject();
                    var ouEntry = new DirectoryEntry($"LDAP://{_config["ActiveDirectory:Domain"]}/{ouPath}");
                    userEntry.MoveTo(ouEntry);
                    userEntry.CommitChanges();
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fejl ved tildeling af bruger til OU: {ex.Message}");
            return false;
        }
    }

    public bool RemoveUserFromOU(string username)
    {
        try
        {
            using (var context = new PrincipalContext(ContextType.Domain, _config["ActiveDirectory:Domain"], _config["ActiveDirectory:ContainerOU"]))
            {
                var user = UserPrincipal.FindByIdentity(context, username);
                if (user != null)
                {
                    var userEntry = (DirectoryEntry)user.GetUnderlyingObject();
                    var defaultOU = new DirectoryEntry($"LDAP://{_config["ActiveDirectory:Domain"]}/{_config["ActiveDirectory:ContainerOU"]}");
                    userEntry.MoveTo(defaultOU);
                    userEntry.CommitChanges();
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fejl ved fjernelse af bruger fra OU: {ex.Message}");
            return false;
        }
    }

    public List<string> GetUserGroups(string samAccountName)
    {
        var groups = new List<string>();
        try
        {
            using (var context = new PrincipalContext(ContextType.Domain, _config["ActiveDirectory:Domain"]))
            {
                var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, samAccountName);
                if (user != null)
                {
                    var userGroups = user.GetGroups();
                    foreach (var group in userGroups)
                    {
                        groups.Add(group.Name);
                    }
                }
            }
        }
        catch (PlatformNotSupportedException)
        {
            // Re-throw so caller can handle platform-specific fallback
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fejl ved hentning af brugergrupper: {ex.Message}");
        }
        return groups;
    }

}