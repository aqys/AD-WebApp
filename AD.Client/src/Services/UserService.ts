import type { AxiosResponse, AxiosError } from "axios";
import axios from "axios";



axios.defaults.withCredentials = true;

function logError(context: string, err: unknown) {
    const e = err as AxiosError;
    if (e.response) {
        console.error(`[UserService] ${context} → HTTP ${e.response.status}`, e.response.data);
    } else {
        console.error(`[UserService] ${context}`, err);
    }
}

export default class UserService {

    

    public async getUsers(): Promise<ADUser[]> {
        try {
            const response: AxiosResponse<ADUser[]> = await axios({
                url: "/api/User",
                method: "GET"
            });
            return Array.isArray(response.data) ? response.data : [];
        } catch (err) {
            logError("getUsers", err);
            return [];
        }
    }

    public async createUser(dto: { username: string; firstName: string; lastName: string; password: string }): Promise<boolean> {
        try {
            const response: AxiosResponse = await axios({
                url: "/api/User/create",
                method: "POST",
                data: JSON.stringify(dto),
                headers: { "Content-Type": "application/json" }
            });
            return response.status === 200;
        } catch (err) {
            logError("createUser", err);
            return false;
        }
    }

    public async disableUser(username: string): Promise<boolean> {
        try {
            const response: AxiosResponse = await axios({
                url: `/api/User/disable/${encodeURIComponent(username)}`,
                method: "POST"
            });
            return response.status === 200;
        } catch (err) {
            logError("disableUser", err);
            return false;
        }
    }

    public async enableUser(username: string): Promise<boolean> {
        try {
            const response: AxiosResponse = await axios({
                url: `/api/User/enable/${encodeURIComponent(username)}`,
                method: "POST"
            });
            return response.status === 200;
        } catch (err) {
            logError("enableUser", err);
            return false;
        }
    }

    public async changePassword(username: string, newPassword: string): Promise<boolean> {
        try {
            const response: AxiosResponse = await axios({
                url: "/api/User/change-password",
                method: "POST",
                data: JSON.stringify({ username, newPassword }),
                headers: { "Content-Type": "application/json" }
            });
            return response.status === 200;
        } catch (err) {
            logError("changePassword", err);
            return false;
        }
    }

    public async changeDisplayName(username: string, firstName: string, lastName: string): Promise<boolean> {
        try {
            const response: AxiosResponse = await axios({
                url: "/api/User/change-displayname",
                method: "POST",
                data: JSON.stringify({ username, firstName, lastName }),
                headers: { "Content-Type": "application/json" }
            });
            return response.status === 200;
        } catch (err) {
            logError("changeDisplayName", err);
            return false;
        }
    }

    

    public async getOUs(): Promise<string[]> {
        try {
            const response: AxiosResponse<string[]> = await axios({
                url: "/api/User/ous",
                method: "GET"
            });
            return Array.isArray(response.data) ? response.data : [];
        } catch (err) {
            logError("getOUs", err);
            return [];
        }
    }

    public async createOU(ouName: string, parentPath: string): Promise<boolean> {
        try {
            const response: AxiosResponse = await axios({
                url: "/api/User/create-ou",
                method: "POST",
                data: JSON.stringify({ ouName, parentPath }),
                headers: { "Content-Type": "application/json" }
            });
            return response.status === 200;
        } catch (err) {
            logError("createOU", err);
            return false;
        }
    }

    public async assignUserToOU(username: string, ouPath: string): Promise<boolean> {
        try {
            const response: AxiosResponse = await axios({
                url: "/api/User/assign-ou",
                method: "POST",
                data: JSON.stringify({ username, ouPath }),
                headers: { "Content-Type": "application/json" }
            });
            return response.status === 200;
        } catch (err) {
            logError("assignUserToOU", err);
            return false;
        }
    }

    public async removeUserFromOU(username: string): Promise<boolean> {
        try {
            const response: AxiosResponse = await axios({
                url: `/api/User/remove-ou/${encodeURIComponent(username)}`,
                method: "POST"
            });
            return response.status === 200;
        } catch (err) {
            logError("removeUserFromOU", err);
            return false;
        }
    }
}