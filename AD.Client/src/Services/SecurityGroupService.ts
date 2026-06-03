import type { AxiosResponse, AxiosError } from "axios";
import axios from "axios";

axios.defaults.withCredentials = true;

function logError(context: string, err: unknown) {
    const e = err as AxiosError;
    if (e.response) {
        console.error(`[SecurityGroupService] ${context} → HTTP ${e.response.status}`, e.response.data);
    } else {
        console.error(`[SecurityGroupService] ${context}`, err);
    }
}

export default class SecurityGroupService {

    public async getGroups(): Promise<ADSecurityGroup[]> {
        try {
            const response: AxiosResponse<ADSecurityGroup[]> = await axios({
                url: "/api/SecurityGroup",
                method: "GET"
            });
            return Array.isArray(response.data) ? response.data : [];
        } catch (err) {
            logError("getGroups", err);
            return [];
        }
    }

    public async createGroup(groupName: string, parentPath: string): Promise<boolean> {
        try {
            const response: AxiosResponse = await axios({
                url: "/api/SecurityGroup/create",
                method: "POST",
                data: JSON.stringify({ groupName, parentPath }),
                headers: { "Content-Type": "application/json" }
            });
            return response.status === 200;
        } catch (err) {
            logError("createGroup", err);
            return false;
        }
    }

    public async deleteGroup(distinguishedName: string): Promise<boolean> {
        try {
            const response: AxiosResponse = await axios({
                url: "/api/SecurityGroup/delete",
                method: "POST",
                data: JSON.stringify({ distinguishedName }),
                headers: { "Content-Type": "application/json" }
            });
            return response.status === 200;
        } catch (err) {
            logError("deleteGroup", err);
            return false;
        }
    }

    public async getMembers(groupDn: string): Promise<string[]> {
        try {
            const response: AxiosResponse<string[]> = await axios({
                url: `/api/SecurityGroup/members?groupDn=${encodeURIComponent(groupDn)}`,
                method: "GET"
            });
            return Array.isArray(response.data) ? response.data : [];
        } catch (err) {
            logError("getMembers", err);
            return [];
        }
    }

    public async addMember(groupDn: string, userDn: string): Promise<boolean> {
        try {
            const response: AxiosResponse = await axios({
                url: "/api/SecurityGroup/add-member",
                method: "POST",
                data: JSON.stringify({ groupDn, userDn }),
                headers: { "Content-Type": "application/json" }
            });
            return response.status === 200;
        } catch (err) {
            logError("addMember", err);
            return false;
        }
    }

    public async removeMember(groupDn: string, userDn: string): Promise<boolean> {
        try {
            const response: AxiosResponse = await axios({
                url: "/api/SecurityGroup/remove-member",
                method: "POST",
                data: JSON.stringify({ groupDn, userDn }),
                headers: { "Content-Type": "application/json" }
            });
            return response.status === 200;
        } catch (err) {
            logError("removeMember", err);
            return false;
        }
    }
}
