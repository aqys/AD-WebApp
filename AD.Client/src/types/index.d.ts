


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

interface DhcpLease {
    ipAddress: string;
    hostName: string;
    macAddress: string;
    leaseExpires: string;
    scopeId: string;
    vlan: string;
    server: string;
}

interface DhcpScope {
    scopeId: string;
    name: string;
    subnetMask: string;
    startRange: string;
    endRange: string;
    state: string;
    vlan: string;
    server: string;
    activeLeases: number;
}
