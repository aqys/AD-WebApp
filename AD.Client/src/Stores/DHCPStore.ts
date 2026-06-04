import DhcpService from "@/Services/DhcpService";
import { defineStore } from "pinia";
import { ref } from "vue";

export const useDhcpStore = defineStore("dhcp", () => {
    const dhcpService = new DhcpService();

    const Leases = ref<DhcpLease[]>([]);
    const Scopes = ref<DhcpScope[]>([]);
    const loading = ref(false);
    const error = ref<string | null>(null);

    async function GET_LEASES() {
        loading.value = true;
        error.value = null;
        try {
            const data = await dhcpService.getLeases();
            Leases.value = data;
            return data;
        } catch {
            error.value = "Kunne ikke hente DHCP-leases";
            return [];
        } finally {
            loading.value = false;
        }
    }

    async function GET_SCOPES() {
        loading.value = true;
        error.value = null;
        try {
            const data = await dhcpService.getScopes();
            Scopes.value = data;
            return data;
        } catch {
            error.value = "Kunne ikke hente DHCP-scopes";
            return [];
        } finally {
            loading.value = false;
        }
    }

    async function REFRESH() {
        loading.value = true;
        error.value = null;
        try {
            const [leases, scopes] = await Promise.all([
                dhcpService.getLeases(),
                dhcpService.getScopes(),
            ]);
            Leases.value = leases;
            Scopes.value = scopes;
        } catch {
            error.value = "Kunne ikke hente DHCP-data";
        } finally {
            loading.value = false;
        }
    }

    return {
        Leases,
        Scopes,
        loading,
        error,
        GET_LEASES,
        GET_SCOPES,
        REFRESH,
    };
});
