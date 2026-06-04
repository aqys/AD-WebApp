import type { AxiosResponse, AxiosError } from "axios";
import axios from "axios";

axios.defaults.withCredentials = true;

function logError(context: string, err: unknown) {
    const e = err as AxiosError;
    if (e.response) {
        console.error(`[AuditLogService] ${context} → HTTP ${e.response.status}`, e.response.data);
    } else {
        console.error(`[AuditLogService] ${context}`, err);
    }
}

export default class AuditLogService {
    public async getAuditLogs(): Promise<AuditLogEntry[]> {
        try {
            const response: AxiosResponse<AuditLogEntry[]> = await axios({
                url: "/api/AuditLog",
                method: "GET"
            });
            return Array.isArray(response.data) ? response.data : [];
        } catch (err) {
            logError("getAuditLogs", err);
            return [];
        }
    }
}
