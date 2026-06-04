import AuditLogService from "@/Services/AuditLogService";
import { defineStore } from "pinia";
import { ref } from "vue";

export const useAuditLogStore = defineStore("auditLog", () => {
    const auditLogService = new AuditLogService();

    const Logs = ref<AuditLogEntry[]>([]);
    const loading = ref(false);
    const error = ref<string | null>(null);

    async function GET_LOGS() {
        loading.value = true;
        error.value = null;
        try {
            const data = await auditLogService.getAuditLogs();
            Logs.value = data;
            return data;
        } catch {
            error.value = "Kunne ikke hente audit-logs";
            return [];
        } finally {
            loading.value = false;
        }
    }

    return {
        Logs,
        loading,
        error,
        GET_LOGS,
    };
});
