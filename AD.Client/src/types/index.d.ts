


interface ADUser {
    username: string;
    firstName: string;
    lastName: string;
    distinguishedName: string;
    ouPath: string | null;
    isEnabled: boolean;
}


interface ADOU {
    
    distinguishedName: string;
    
    name: string;
}

interface ADSecurityGroup {
    name: string;
    distinguishedName: string;
}
