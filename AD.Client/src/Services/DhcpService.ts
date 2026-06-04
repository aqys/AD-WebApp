import type { AxiosResponse, AxiosError } from "axios";
import axios from "axios";

axios.defaults.withCredentials = true;

function logError(context: string, err: unknown) {
    const e = err as AxiosError;
    if (e.response) {
        console.error(`[DhcpService] ${context} → HTTP ${e.response.status}`, e.response.data);
    } else {
        console.error(`[DhcpService] ${context}`, err);
    }
}

export default class DhcpService {

    public async getLeases(): Promise<DhcpLease[]> {
        try {
            const response: AxiosResponse<DhcpLease[]> = await axios({
                url: "/api/Dhcp/leases",
                method: "GET"
            });
            return Array.isArray(response.data) ? response.data : [];
        } catch (err) {
            logError("getLeases", err);
            return [];
        }
    }

    public async getScopes(): Promise<DhcpScope[]> {
        try {
            const response: AxiosResponse<DhcpScope[]> = await axios({
                url: "/api/Dhcp/scopes",
                method: "GET"
            });
            return Array.isArray(response.data) ? response.data : [];
        } catch (err) {
            logError("getScopes", err);
            return [];
        }
    }
}
