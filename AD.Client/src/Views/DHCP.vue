<template>
    <Back icon="network-wired" title="DHCP Overblik" />
    <ManagementWrapper>
        <Workspace :filled="true">
            <section class="hero is-link">
                <div class="hero-body">
                    <p class="title is-3">DHCP Overblik</p>
                    <p class="subtitle is-5">Aktive scopes og leases</p>
                </div>
            </section>
            <WorkspaceContent>
                <div class="dhcp-container">
                    <div class="level mb-5">
                        <div class="level-left">
                            <div class="level-item">
                                <BInput
                                    class="custom-search-input"
                                    v-model="searchQuery"
                                    icon="magnifying-glass"
                                    placeholder="Søg på hostname, IP eller MAC..."
                                    style="width: 320px;"
                                />
                            </div>
                            <div class="level-item">
                                <div class="buttons">
                                    <button
                                        v-for="vlan in vlanOptions"
                                        :key="vlan"
                                        class="button is-rounded is-small"
                                        :class="activeVlan === vlan ? 'is-link' : 'is-light'"
                                        @click="setVlan(vlan)"
                                    >
                                        {{ vlan }}
                                    </button>
                                </div>
                            </div>
                        </div>
                        <div class="level-right">
                            <div class="level-item" v-if="lastRefreshed">
                                <span class="has-text-grey is-size-7">
                                    <font-awesome-icon icon="clock" class="mr-1" />
                                    Opdateret {{ lastRefreshed }}
                                </span>
                            </div>
                            <div class="level-item">
                                <BButton
                                    type="is-light"
                                    :loading="dhcpStore.loading"
                                    icon-left="arrows-rotate"
                                    @click="refresh"
                                >
                                    Opdater
                                </BButton>
                            </div>
                        </div>
                    </div>

                    <div class="columns mb-5" v-if="filteredScopes.length > 0">
                        <div
                            v-for="scope in filteredScopes"
                            :key="scope.scopeId + scope.server"
                            class="column is-4"
                        >
                            <nav class="panel" :class="getPanelColorClass(scope.vlan)">
                                <p class="panel-heading has-text-white">
                                    {{ scope.name || scope.scopeId }}
                                </p>
                                <div class="panel-block is-block py-3 px-4">
                                    <div>
                                        <div style="display: flex; justify-content: space-between; align-items: center; padding: 0.5rem 0; border-bottom: 1px solid rgba(0,0,0,0.05);">
                                            <span class="has-text-grey is-size-7 has-text-weight-bold">SCOPE ID</span>
                                            <span class="is-size-7 font-family-mono">{{ scope.scopeId }}</span>
                                        </div>
                                        <div style="display: flex; justify-content: space-between; align-items: center; padding: 0.5rem 0; border-bottom: 1px solid rgba(0,0,0,0.05);">
                                            <span class="has-text-grey is-size-7 has-text-weight-bold">INTERVAL</span>
                                            <span class="is-size-7 font-family-mono">{{ scope.startRange }} – {{ scope.endRange }}</span>
                                        </div>
                                        <div style="display: flex; justify-content: space-between; align-items: center; padding: 0.5rem 0; border-bottom: 1px solid rgba(0,0,0,0.05);">
                                            <span class="has-text-grey is-size-7 has-text-weight-bold">SERVER</span>
                                            <span class="is-size-7">{{ scope.server }}</span>
                                        </div>
                                        <div style="display: flex; justify-content: space-between; align-items: center; padding: 0.5rem 0;">
                                            <span class="has-text-grey is-size-7 has-text-weight-bold">AKTIVE LEASES</span>
                                            <span class="is-size-6 has-text-weight-bold">{{ scope.activeLeases }}</span>
                                        </div>
                                    </div>
                                </div>
                            </nav>
                        </div>
                    </div>

                    <nav class="panel">
                        <p class="panel-heading is-flex is-justify-content-between is-align-items-center">
                            <span>
                                <font-awesome-icon icon="list" class="mr-2" />
                                Aktive DHCP Leases
                                <span class="tag is-dark ml-2">{{ filteredLeases.length }}</span>
                            </span>
                            <span class="tags" v-if="uniqueServers.length > 1">
                                <span
                                    v-for="server in uniqueServers"
                                    :key="server"
                                    class="tag is-clickable"
                                    :class="activeServer === server ? 'is-dark' : 'is-light'"
                                    @click="toggleServer(server)"
                                >
                                    <font-awesome-icon icon="server" class="mr-1" />
                                    {{ server }}
                                </span>
                            </span>
                        </p>

                        <div v-if="dhcpStore.loading" class="panel-block py-6 is-justify-content-center has-text-grey">
                            <div class="loader mr-3"></div>
                            <span>Henter DHCP-data fra serverne…</span>
                        </div>

                        <div v-else-if="dhcpStore.error" class="panel-block py-6 is-justify-content-center has-text-danger">
                            <font-awesome-icon icon="triangle-exclamation" class="mr-2" />
                            {{ dhcpStore.error }}
                        </div>

                        <div v-else-if="filteredLeases.length === 0 && !dhcpStore.loading" class="panel-block py-6 is-justify-content-center has-text-grey">
                            <font-awesome-icon icon="inbox" class="mr-2" />
                            <span>Ingen leases fundet</span>
                        </div>

                        <div v-else class="panel-block p-0 table-container-block">
                            <table class="table is-striped is-hoverable is-fullwidth mb-0">
                                <thead>
                                    <tr>
                                        <th @click="sortBy('ipAddress')" class="is-clickable">
                                            IP-adresse
                                            <font-awesome-icon :icon="getSortIcon('ipAddress')" class="ml-1" />
                                        </th>
                                        <th @click="sortBy('hostName')" class="is-clickable">
                                            Hostname
                                            <font-awesome-icon :icon="getSortIcon('hostName')" class="ml-1" />
                                        </th>
                                        <th @click="sortBy('macAddress')" class="is-clickable">
                                            MAC-adresse
                                            <font-awesome-icon :icon="getSortIcon('macAddress')" class="ml-1" />
                                        </th>
                                        <th @click="sortBy('vlan')" class="is-clickable">
                                            VLAN
                                            <font-awesome-icon :icon="getSortIcon('vlan')" class="ml-1" />
                                        </th>
                                        <th @click="sortBy('server')" class="is-clickable">
                                            Server
                                            <font-awesome-icon :icon="getSortIcon('server')" class="ml-1" />
                                        </th>
                                        <th @click="sortBy('leaseExpires')" class="is-clickable">
                                            Udløber
                                            <font-awesome-icon :icon="getSortIcon('leaseExpires')" class="ml-1" />
                                        </th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr v-for="lease in filteredLeases" :key="lease.ipAddress + lease.server">
                                        <td class="font-family-mono">
                                            <font-awesome-icon icon="circle" class="mr-2" :class="getVlanDotClass(lease.vlan)" />
                                            {{ lease.ipAddress }}
                                        </td>
                                        <td class="has-text-weight-semibold">{{ lease.hostName || '–' }}</td>
                                        <td class="font-family-mono is-size-7 has-text-grey">{{ lease.macAddress || '–' }}</td>
                                        <td>
                                            <span class="tag" :class="getVlanTagClass(lease.vlan)">
                                                {{ lease.vlan }}
                                            </span>
                                        </td>
                                        <td class="is-size-7 has-text-grey">{{ lease.server }}</td>
                                        <td class="is-size-7 has-text-grey">{{ lease.leaseExpires || '–' }}</td>
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
import { useDhcpStore } from '@/Stores/DHCPStore';
import { BInput, BButton } from 'buefy';
import Back from '@/Components/Back.vue';
import ManagementWrapper from '@/Components/ManagementWrapper.vue';
import Workspace from '@/Components/Workspace/Workspace.vue';
import WorkspaceContent from '@/Components/Workspace/WorkspaceContent.vue';

const dhcpStore = useDhcpStore();
const { Leases: leases, Scopes: scopes } = storeToRefs(dhcpStore);

const searchQuery = ref('');
const activeVlan = ref('Alle');
const activeServer = ref<string | null>(null);
const sortKey = ref<keyof DhcpLease>('ipAddress');
const sortDir = ref<'asc' | 'desc'>('asc');
const lastRefreshed = ref<string | null>(null);

const vlanOptions = computed(() => {
    return ['Alle', ...new Set(leases.value.map(l => l.vlan))];
});

const uniqueServers = computed(() => [...new Set(leases.value.map(l => l.server))]);

const filteredScopes = computed(() => {
    if (activeVlan.value === 'Alle') return scopes.value;
    return scopes.value.filter(s => s.vlan === activeVlan.value);
});

const filteredLeases = computed(() => {
    let list = leases.value;

    if (activeVlan.value !== 'Alle') {
        list = list.filter(l => l.vlan === activeVlan.value);
    }

    if (activeServer.value) {
        list = list.filter(l => l.server === activeServer.value);
    }

    const q = searchQuery.value.trim().toLowerCase();
    if (q) {
        list = list.filter(l =>
            l.ipAddress.toLowerCase().includes(q) ||
            l.hostName.toLowerCase().includes(q) ||
            l.macAddress.toLowerCase().includes(q)
        );
    }

    return [...list].sort((a, b) => {
        const av = a[sortKey.value] ?? '';
        const bv = b[sortKey.value] ?? '';
        const cmp = String(av).localeCompare(String(bv), 'da', { numeric: true });
        return sortDir.value === 'asc' ? cmp : -cmp;
    });
});

const setVlan = (vlan: string) => {
    activeVlan.value = vlan;
    activeServer.value = null;
};

const toggleServer = (server: string) => {
    activeServer.value = activeServer.value === server ? null : server;
};

const sortBy = (key: keyof DhcpLease) => {
    if (sortKey.value === key) {
        sortDir.value = sortDir.value === 'asc' ? 'desc' : 'asc';
    } else {
        sortKey.value = key;
        sortDir.value = 'asc';
    }
};

const getSortIcon = (key: keyof DhcpLease) => {
    if (sortKey.value !== key) return 'sort';
    return sortDir.value === 'asc' ? 'sort-up' : 'sort-down';
};

const getVlanTagClass = (vlan: string) => {
    if (vlan.includes('10')) return 'is-link is-light';
    if (vlan.includes('20')) return 'is-primary is-light';
    if (vlan.includes('30')) return 'is-success is-light';
    if (vlan.includes('99')) return 'is-warning is-light';
    return 'is-light';
};

const getVlanDotClass = (vlan: string) => {
    if (vlan.includes('10')) return 'has-text-link';
    if (vlan.includes('20')) return 'has-text-primary';
    if (vlan.includes('30')) return 'has-text-success';
    if (vlan.includes('99')) return 'has-text-warning';
    return 'has-text-grey-light';
};

const getPanelColorClass = (vlan: string) => {
    if (vlan.includes('10')) return 'is-link';
    if (vlan.includes('20')) return 'is-primary';
    if (vlan.includes('30')) return 'is-success';
    if (vlan.includes('99')) return 'is-warning';
    return '';
};

const refresh = async () => {
    await dhcpStore.REFRESH();
    const now = new Date();
    lastRefreshed.value = now.toLocaleTimeString('da-DK', { hour: '2-digit', minute: '2-digit' });
};

onMounted(async () => {
    await refresh();
});
</script>

<style lang="scss">
.dhcp-container {
    padding: 1.5rem;
}

.table-container-block {
    width: 100%;
    overflow-x: auto;
    display: block !important;
}

.font-family-mono {
    font-family: 'JetBrains Mono', 'Fira Code', monospace;
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
