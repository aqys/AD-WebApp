<template>
    <Back icon="clock-rotate-left" title="Audit Logs" />
    <ManagementWrapper>
        <Workspace :filled="true">
            <section class="hero is-link">
                <div class="hero-body">
                    <p class="title is-3">Audit Logs</p>
                    <p class="subtitle is-5">Centraliseret oversigt over kritiske handlinger i Active Directory</p>
                </div>
            </section>
            <WorkspaceContent>
                <div class="logs-container">
                    <div class="level mb-6">
                        <div class="level-left">
                            <div class="level-item">
                                <BInput
                                    class="custom-search-input"
                                    v-model="searchQuery"
                                    icon="magnifying-glass"
                                    placeholder="Søg i loggen (Admin, handling, mål...)"
                                    style="width: 320px;"
                                />
                            </div>
                            <div class="level-item">
                                <div class="buttons">
                                    <button
                                        v-for="cat in categories"
                                        :key="cat.value"
                                        class="button is-rounded"
                                        :class="activeCategory === cat.value ? 'is-link' : 'is-light'"
                                        @click="activeCategory = cat.value"
                                    >
                                        {{ cat.name }}
                                    </button>
                                </div>
                            </div>
                        </div>
                        <div class="level-right">
                            <div class="level-item">
                                <div class="buttons">
                                    <BButton
                                        type="is-info"
                                        outlined
                                        icon-left="download"
                                        @click="exportLogs"
                                        :disabled="filteredLogs.length === 0"
                                    >
                                        Eksporter som JSON
                                    </BButton>
                                    <BButton
                                        type="is-light"
                                        :loading="auditLogStore.loading"
                                        icon-left="arrows-rotate"
                                        @click="refresh"
                                    >
                                        Opdater
                                    </BButton>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- Logs Table Panel -->
                    <nav class="panel">
                        <p class="panel-heading is-flex is-justify-content-between is-align-items-center">
                            <span>
                                <font-awesome-icon icon="list" class="mr-2" />
                                Registrerede Hændelser
                                <span class="tag is-dark ml-2">{{ filteredLogs.length }}</span>
                            </span>
                        </p>

                        <!-- Loading State -->
                        <div v-if="auditLogStore.loading" class="panel-block py-6 is-justify-content-center has-text-grey">
                            <div class="loader mr-3"></div>
                            <span>Henter revisionslogge fra serveren…</span>
                        </div>

                        <!-- Error State -->
                        <div v-else-if="auditLogStore.error" class="panel-block py-6 is-justify-content-center has-text-danger">
                            <font-awesome-icon icon="triangle-exclamation" class="mr-2" />
                            {{ auditLogStore.error }}
                        </div>

                        <!-- Empty State -->
                        <div v-else-if="filteredLogs.length === 0" class="panel-block py-6 is-justify-content-center has-text-grey">
                            <div class="has-text-centered py-4">
                                <font-awesome-icon icon="inbox" size="2x" class="mb-3 has-text-grey-light" />
                                <p class="is-size-6">Ingen loghændelser fundet</p>
                                <p class="is-size-7 has-text-grey-light">Prøv at ændre dine søgekriterier eller filterkategorier</p>
                            </div>
                        </div>

                        <!-- Table Content -->
                        <div v-else class="panel-block p-0 table-container-block">
                            <table class="table is-striped is-hoverable is-fullwidth mb-0">
                                <thead>
                                    <tr>
                                        <th @click="sortBy('timestamp')" class="is-clickable" style="width: 200px;">
                                            Tidspunkt
                                            <font-awesome-icon :icon="getSortIcon('timestamp')" class="ml-1" />
                                        </th>
                                        <th @click="sortBy('administrator')" class="is-clickable" style="width: 220px;">
                                            Administrator
                                            <font-awesome-icon :icon="getSortIcon('administrator')" class="ml-1" />
                                        </th>
                                        <th @click="sortBy('action')" class="is-clickable" style="width: 200px;">
                                            Handling
                                            <font-awesome-icon :icon="getSortIcon('action')" class="ml-1" />
                                        </th>
                                        <th @click="sortBy('target')" class="is-clickable" style="width: 180px;">
                                            Mål (Object)
                                            <font-awesome-icon :icon="getSortIcon('target')" class="ml-1" />
                                        </th>
                                        <th>Detaljer</th>
                                        <th style="width: 130px;">IP-adresse</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr v-for="log in filteredLogs" :key="log.timestamp + log.target + log.action">
                                        <td class="font-family-mono has-text-grey-dark">
                                            {{ formatDateTime(log.timestamp) }}
                                        </td>
                                        <td class="has-text-weight-semibold">
                                            <div class="is-flex is-align-items-center">
                                                <font-awesome-icon icon="user-shield" class="mr-2 has-text-grey-light" />
                                                <span>{{ log.administrator }}</span>
                                            </div>
                                        </td>
                                        <td>
                                            <span class="tag" :class="getActionTagClass(log.action)">
                                                {{ log.action }}
                                            </span>
                                        </td>
                                        <td>
                                            <code class="font-family-mono target-code-block">{{ log.target }}</code>
                                        </td>
                                        <td class="has-text-grey-dark">
                                            {{ log.details }}
                                        </td>
                                        <td class="font-family-mono has-text-grey">
                                            {{ log.ipAddress }}
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </nav>
                </div>
            </WorkspaceContent>
        </Workspace>
    </ManagementWrapper>
</template>

<script lang="ts" setup>
import { ref, computed, onMounted } from 'vue';
import { storeToRefs } from 'pinia';
import { useAuditLogStore } from '@/Stores/AuditLogStore';
import { BInput, BButton } from 'buefy';
import Back from '@/Components/Back.vue';
import ManagementWrapper from '@/Components/ManagementWrapper.vue';
import Workspace from '@/Components/Workspace/Workspace.vue';
import WorkspaceContent from '@/Components/Workspace/WorkspaceContent.vue';

const auditLogStore = useAuditLogStore();
const { Logs: logs } = storeToRefs(auditLogStore);

const searchQuery = ref('');
const activeCategory = ref('all');
const sortKey = ref<keyof AuditLogEntry>('timestamp');
const sortDir = ref<'asc' | 'desc'>('desc');

const categories = [
    { name: 'Alle', value: 'all' },
    { name: 'Brugere', value: 'users' },
    { name: 'OUs', value: 'ous' },
    { name: 'Sikkerhedsgrupper', value: 'groups' }
];

const formatDateTime = (isoString: string) => {
    try {
        const d = new Date(isoString);
        return d.toLocaleString('da-DK', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit'
        });
    } catch {
        return isoString;
    }
};

const getActionTagClass = (action: string) => {
    const act = action.toLowerCase();
    if (act.includes('oprettet') || act.includes('aktiveret')) {
        return 'is-success is-light';
    }
    if (act.includes('deaktiveret') || act.includes('slettet')) {
        return 'is-danger is-light';
    }
    if (act.includes('adgangskode')) {
        return 'is-warning is-light has-text-warning-dark';
    }
    if (act.includes('medlem')) {
        return 'is-info is-light';
    }
    return 'is-link is-light';
};

const isInCategory = (action: string, category: string) => {
    if (category === 'all') return true;
    const act = action.toLowerCase();
    if (category === 'users') {
        return act.includes('bruger') || act.includes('adgangskode') || act.includes('visningsnavn');
    }
    if (category === 'ous') {
        return act.includes('ou');
    }
    if (category === 'groups') {
        return act.includes('gruppe') || act.includes('medlem');
    }
    return false;
};

const filteredLogs = computed(() => {
    let list = logs.value;

    // Filter by Category
    if (activeCategory.value !== 'all') {
        list = list.filter(l => isInCategory(l.action, activeCategory.value));
    }

    // Filter by Search Query
    const q = searchQuery.value.trim().toLowerCase();
    if (q) {
        list = list.filter(l =>
            l.administrator.toLowerCase().includes(q) ||
            l.action.toLowerCase().includes(q) ||
            l.target.toLowerCase().includes(q) ||
            l.details.toLowerCase().includes(q) ||
            l.ipAddress.toLowerCase().includes(q)
        );
    }

    // Sort logs
    return [...list].sort((a, b) => {
        const av = a[sortKey.value] ?? '';
        const bv = b[sortKey.value] ?? '';
        const cmp = String(av).localeCompare(String(bv), 'da', { numeric: true });
        return sortDir.value === 'asc' ? cmp : -cmp;
    });
});

const sortBy = (key: keyof AuditLogEntry) => {
    if (sortKey.value === key) {
        sortDir.value = sortDir.value === 'asc' ? 'desc' : 'asc';
    } else {
        sortKey.value = key;
        sortDir.value = 'asc';
    }
};

const getSortIcon = (key: keyof AuditLogEntry) => {
    if (sortKey.value !== key) return 'sort';
    return sortDir.value === 'asc' ? 'sort-up' : 'sort-down';
};

const exportLogs = () => {
    try {
        const jsonStr = JSON.stringify(logs.value, null, 2);
        const blob = new Blob([jsonStr], { type: 'application/json' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `audit_logs_${new Date().toISOString().split('T')[0]}.json`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    } catch (err) {
        console.error('Error exporting logs:', err);
    }
};

const refresh = async () => {
    await auditLogStore.GET_LOGS();
};

onMounted(async () => {
    await refresh();
});
</script>

<style lang="scss">
.logs-container {
    padding: 1.5rem;
}

.table-container-block {
    width: 100%;
    overflow-x: auto;
    display: block !important;
}

.font-family-mono {
    font-family: 'JetBrains Mono', 'Fira Code', monospace;
    font-size: 0.9rem;
}

.table td, .table th {
    padding: 0.75rem 1rem !important;
    vertical-align: middle !important;
    font-size: 0.95rem;
}

.table th {
    font-weight: 600;
    color: #475569 !important;
    background-color: #f8fafc;
    border-bottom: 2px solid #e2e8f0 !important;
}

.table tbody tr {
    transition: background-color 0.2s ease;
}

.table tbody tr:hover {
    background-color: #f8fafc !important;
}

.target-code-block {
    background-color: #f1f5f9;
    color: #0f172a;
    padding: 0.25rem 0.5rem;
    border-radius: 6px;
    font-size: 0.85rem;
    white-space: nowrap;
    max-width: 200px;
    display: inline-block;
    vertical-align: middle;
    overflow: hidden;
    text-overflow: ellipsis;
    border: 1px solid rgba(0, 0, 0, 0.05);
}

.has-text-warning-dark {
    color: #854d0e !important;
}

.custom-search-input {
    input {
        background-color: #fff;
        color: #000;
        border: none;
        border-radius: 6px;
        box-shadow: none;
        border: 1px solid rgba(0, 0, 0, 0.1);
        
        &::placeholder {
            color: #475569;
            font-weight: 500;
        }
        
        &:focus {
            box-shadow: inset 0 0 0 1px #334155;
            background-color: #f1f5f9;
        }
    }
    
    .icon {
        color: #475569 !important;
        svg {
            width: 1rem;
            height: 1rem;
        }
    }
}
</style>
