<template>
    <Back icon="users-gear" title="Sikkerheds Grupper" />
    <ManagementWrapper>
        <Sidebar>
            <SidebarHeader>
                <BInput
                    class="custom-search-input"
                    ref="searchInput"
                    v-model="searchQuery"
                    icon="magnifying-glass"
                    placeholder="Søg på gruppenavn"
                />
            </SidebarHeader>
            <SidebarContent>
                <SGSidebarBox
                    v-for="group in filteredGroups"
                    :key="group.distinguishedName"
                    :group="group"
                    :selected="selectedGroup?.distinguishedName === group.distinguishedName"
                    @click="toggleGroupSelection(group)"
                />
            </SidebarContent>
            <SidebarFooter>
                <SGCreateGroup />
            </SidebarFooter>
        </Sidebar>
        <Workspace :filled="selectedGroup != null">
            <section class="hero is-link" v-if="selectedGroup">
                <div class="hero-body">
                    <p class="title is-3">{{ selectedGroup.name }}</p>
                    <p class="subtitle is-5">{{ selectedGroup.distinguishedName }}</p>
                </div>
            </section>
            <WorkspaceContent v-if="selectedGroup">
                <div class="columns is-desktop">
                    <div class="column">
                        <SGMetadata
                            :selected-group="selectedGroup"
                            @deleted="onGroupDeleted"
                        />
                    </div>
                    <div class="column">
                        <SGMembers :selected-group="selectedGroup" />
                    </div>
                </div>
            </WorkspaceContent>
        </Workspace>
    </ManagementWrapper>
</template>
<script lang="ts" setup>
import { ref, computed, onMounted, onUnmounted } from 'vue';
import { useUserStore } from '@/Stores/UserStore';
import { storeToRefs } from 'pinia';
import { BInput } from 'buefy';
import Back from '@/Components/Back.vue';
import ManagementWrapper from '@/Components/ManagementWrapper.vue';
import Sidebar from '@/Components/Sidebar/Sidebar.vue';
import SidebarHeader from '@/Components/Sidebar/SidebarHeader.vue';
import SidebarContent from '@/Components/Sidebar/SidebarContent.vue';
import SidebarFooter from '@/Components/Sidebar/SidebarFooter.vue';
import Workspace from '@/Components/Workspace/Workspace.vue';
import WorkspaceContent from '@/Components/Workspace/WorkspaceContent.vue';
import SGSidebarBox from '@/Components/SecurityGroups/SGSidebarBox.vue';
import SGCreateGroup from '@/Components/SecurityGroups/SGCreateGroup.vue';
import SGMetadata from '@/Components/SecurityGroups/SGMetadata.vue';
import SGMembers from '@/Components/SecurityGroups/SGMembers.vue';

const userStore = useUserStore();
const { SecurityGroups: groups } = storeToRefs(userStore);

const selectedGroup = ref<ADSecurityGroup | null>(null);
const searchQuery = ref('');

const filteredGroups = computed(() => {
    const q = searchQuery.value.trim().toLowerCase();
    if (!q) return groups.value;
    return groups.value.filter((g: ADSecurityGroup) =>
        g.name.toLowerCase().includes(q) ||
        g.distinguishedName.toLowerCase().includes(q)
    );
});

const toggleGroupSelection = (clicked: ADSecurityGroup) => {
    if (selectedGroup.value?.distinguishedName === clicked.distinguishedName) {
        selectedGroup.value = null;
    } else {
        selectedGroup.value = JSON.parse(JSON.stringify(clicked));
    }
};

const onGroupDeleted = async () => {
    selectedGroup.value = null;
    await userStore.GET_SECURITY_GROUPS();
};

const searchInput = ref<any>(null);
const handleGlobalKeyDown = (e: KeyboardEvent) => {
    if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k') {
        e.preventDefault();
        const el = searchInput.value?.$el?.querySelector('input') ?? searchInput.value;
        el?.focus?.();
    }
};

onMounted(async () => {
    window.addEventListener('keydown', handleGlobalKeyDown);
    await userStore.GET_SECURITY_GROUPS();
});

onUnmounted(() => {
    window.removeEventListener('keydown', handleGlobalKeyDown);
});
</script>
<style lang="scss">
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
