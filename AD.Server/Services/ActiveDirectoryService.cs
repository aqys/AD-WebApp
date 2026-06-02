using System;
using System.Collections.Generic;
using System.Linq;
using AD.Server.Models;
using Microsoft.Extensions.Configuration;
using Novell.Directory.Ldap;

public class ActiveDirectoryService : IActiveDirectoryService
{
    private readonly IConfiguration _config;

    public ActiveDirectoryService(IConfiguration config)
    {
        _config = config;
    }

    private ILdapConnection GetConnection()
    {
        var server = _config["ActiveDirectory:LdapServer"] ?? _config["ActiveDirectory:Domain"];
        var username = _config["ActiveDirectory:Username"];
        var password = _config["ActiveDirectory:Password"];

        var connection = new LdapConnection();
        connection.SecureSocketLayer = false;
        
        // Trust all certs (internal domain CA)
        connection.UserDefinedServerCertValidationDelegate += (sender, cert, chain, errors) => true;

        connection.Connect(server, 389);
        connection.StartTls();

        if (!string.IsNullOrEmpty(username))
        {
            string bindUsername = username;
            if (username.Contains("\\"))
            {
                var parts = username.Split('\\');
                bindUsername = $"{parts[1]}@{parts[0]}.local".ToLower();
            }
            connection.Bind(bindUsername, password);
        }
        else
        {
            connection.Bind(null, null);
        }

        return connection;
    }

    private string GetDomainDn()
    {
        var domain = _config["ActiveDirectory:Domain"];
        if (string.IsNullOrEmpty(domain)) return "";
        return string.Join(",", domain.Split('.').Select(p => $"DC={p}"));
    }

    private string GetDefaultContainer()
    {
        var container = _config["ActiveDirectory:ContainerOU"];
        return string.IsNullOrEmpty(container) ? GetDomainDn() : container;
    }

    private LdapEntry? FindUserEntry(ILdapConnection connection, string samAccountName)
    {
        var domainDn = GetDomainDn();
        var filter = $"(&(objectCategory=person)(objectClass=user)(sAMAccountName={EscapeLdap(samAccountName)}))";
        
        var searchConstraints = new LdapSearchConstraints();
        var results = connection.Search(domainDn, LdapConnection.ScopeSub, filter, new[] { "distinguishedName", "userAccountControl" }, false, searchConstraints);
        
        if (results.HasMore())
        {
            return results.Next();
        }
        return null;
    }

    private static string EscapeLdap(string value) =>
        value.Replace("\\", "\\5c").Replace("*", "\\2a").Replace("(", "\\28").Replace(")", "\\29");

    public List<AdUserDto> GetAllUsers()
    {
        var result = new List<AdUserDto>();
        try
        {
            using var connection = GetConnection();
            var domainDn = GetDomainDn();
            var filter = "(&(objectCategory=person)(objectClass=user))";
            
            var cons = new LdapSearchConstraints();
            cons.MaxResults = 1000;
            
            var results = connection.Search(domainDn, LdapConnection.ScopeSub, filter, new[] { "sAMAccountName", "givenName", "sn", "distinguishedName", "userAccountControl" }, false, cons);
            
            while (results.HasMore())
            {
                try 
                {
                    var entry = results.Next();
                    var sam = GetAttribute(entry, "sAMAccountName");
                    if (string.IsNullOrEmpty(sam)) continue;

                    var dn = entry.Dn;
                    var uacStr = GetAttribute(entry, "userAccountControl");
                    var uac = int.TryParse(uacStr, out var u) ? u : 0;
                    var isDisabled = (uac & 2) != 0;

                    string? ouPath = null;
                    var dnParts = dn.Split(',');
                    if (dnParts.Length > 1)
                        ouPath = string.Join(",", dnParts.Skip(1));

                    result.Add(new AdUserDto
                    {
                        Username = sam,
                        FirstName = GetAttribute(entry, "givenName"),
                        LastName = GetAttribute(entry, "sn"),
                        DistinguishedName = dn,
                        OUPath = ouPath,
                        IsEnabled = !isDisabled
                    });
                } 
                catch (LdapReferralException) 
                {
                    continue; // Skip referrals
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AD] GetAllUsers error: {ex}");
            throw;
        }
        return result;
    }

    private string GetAttribute(LdapEntry entry, string attributeName)
    {
        try 
        {
            var attr = entry.GetAttribute(attributeName);
            if (attr != null)
            {
                return attr.StringValue ?? "";
            }
        } 
        catch (KeyNotFoundException) 
        {
            // Ignore
        }
        return "";
    }

    public bool CreateUser(UserCreateDto dto)
    {
        try
        {
            using var connection = GetConnection();
            var container = GetDefaultContainer();
            var dn = $"CN={EscapeLdap(dto.FirstName)} {EscapeLdap(dto.LastName)},{container}";

            var attributes = new LdapAttributeSet
            {
                new LdapAttribute("objectClass", "user"),
                new LdapAttribute("sAMAccountName", dto.Username),
                new LdapAttribute("givenName", dto.FirstName),
                new LdapAttribute("sn", dto.LastName),
                new LdapAttribute("displayName", $"{dto.FirstName} {dto.LastName}"),
                new LdapAttribute("userPrincipalName", $"{dto.Username}@{_config["ActiveDirectory:Domain"]}"),
                new LdapAttribute("mail", $"{dto.Username}@{_config["ActiveDirectory:Domain"]}"),
                new LdapAttribute("userAccountControl", "512")
            };

            var entry = new LdapEntry(dn, attributes);
            connection.Add(entry);

            // Set password
            var passwordBytes = System.Text.Encoding.Unicode.GetBytes($"\"{dto.Password}\"");
            var mod = new LdapModification(LdapModification.Replace, new LdapAttribute("unicodePwd", passwordBytes));
            connection.Modify(dn, new[] { mod });

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AD] CreateUser error: {ex}");
            return false;
        }
    }

    public bool DisableUser(string samAccountName)
    {
        try
        {
            using var connection = GetConnection();
            var user = FindUserEntry(connection, samAccountName);
            if (user == null) return false;

            var uacStr = GetAttribute(user, "userAccountControl");
            var uac = int.TryParse(uacStr, out var u) ? u : 512;
            uac |= 2; // set ACCOUNTDISABLE bit

            var mod = new LdapModification(LdapModification.Replace, new LdapAttribute("userAccountControl", uac.ToString()));
            connection.Modify(user.Dn, new[] { mod });
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AD] DisableUser error: {ex}");
            return false;
        }
    }

    public bool ChangePassword(string samAccountName, string newPassword)
    {
        try
        {
            using var connection = GetConnection();
            var user = FindUserEntry(connection, samAccountName);
            if (user == null) return false;

            var passwordBytes = System.Text.Encoding.Unicode.GetBytes($"\"{newPassword}\"");
            var mod = new LdapModification(LdapModification.Replace, new LdapAttribute("unicodePwd", passwordBytes));
            connection.Modify(user.Dn, new[] { mod });
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AD] ChangePassword error: {ex}");
            return false;
        }
    }

    public bool ChangeDisplayName(string samAccountName, string firstName, string lastName)
    {
        try
        {
            using var connection = GetConnection();
            var user = FindUserEntry(connection, samAccountName);
            if (user == null) return false;

            var mods = new[]
            {
                new LdapModification(LdapModification.Replace, new LdapAttribute("givenName", firstName)),
                new LdapModification(LdapModification.Replace, new LdapAttribute("sn", lastName)),
                new LdapModification(LdapModification.Replace, new LdapAttribute("displayName", $"{firstName} {lastName}"))
            };
            
            connection.Modify(user.Dn, mods);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AD] ChangeDisplayName error: {ex}");
            return false;
        }
    }

    public List<string> GetAllOUs()
    {
        var result = new List<string>();
        try
        {
            using var connection = GetConnection();
            var domainDn = GetDomainDn();
            var filter = "(objectClass=organizationalUnit)";
            
            var cons = new LdapSearchConstraints();
            cons.MaxResults = 1000;
            
            var results = connection.Search(domainDn, LdapConnection.ScopeSub, filter, new[] { "distinguishedName" }, false, cons);
            
            while (results.HasMore())
            {
                try 
                {
                    var entry = results.Next();
                    result.Add(entry.Dn);
                }
                catch (LdapReferralException)
                {
                    continue;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AD] GetAllOUs error: {ex}");
            throw;
        }
        return result;
    }

    public bool CreateOU(string ouName, string parentPath)
    {
        try
        {
            using var connection = GetConnection();
            var dn = $"OU={EscapeLdap(ouName)},{parentPath}";
            
            var attributes = new LdapAttributeSet
            {
                new LdapAttribute("objectClass", "organizationalUnit")
            };
            
            var entry = new LdapEntry(dn, attributes);
            connection.Add(entry);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AD] CreateOU error: {ex}");
            return false;
        }
    }

    public bool AssignUserToOU(string samAccountName, string ouPath)
    {
        try
        {
            using var connection = GetConnection();
            var user = FindUserEntry(connection, samAccountName);
            if (user == null) return false;

            var oldDn = user.Dn;
            var newParent = ouPath;
            var rdn = oldDn.Substring(0, oldDn.IndexOf(','));

            connection.Rename(oldDn, rdn, newParent, true);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AD] AssignUserToOU error: {ex}");
            return false;
        }
    }

    public bool RemoveUserFromOU(string samAccountName)
    {
        try
        {
            var container = GetDefaultContainer();
            return AssignUserToOU(samAccountName, container);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AD] RemoveUserFromOU error: {ex}");
            return false;
        }
    }
}