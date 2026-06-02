// Global TypeScript declarations for the AD WebApp

/** A user fetched from Active Directory */
interface ADUser {
    username: string;
    firstName: string;
    lastName: string;
    distinguishedName: string;
    ouPath: string | null;
    isEnabled: boolean;
}

/** An Organizational Unit in Active Directory */
interface ADOU {
    /** Full distinguished name, e.g. "OU=Sales,DC=corp,DC=local" */
    distinguishedName: string;
    /** Friendly display name extracted from DN */
    name: string;
}
