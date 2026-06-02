<template>
    <Back icon="sitemap" title="Organisation &amp; OUs" />
    <ManagementWrapper>
        <Sidebar>
            <SidebarHeader>
                <BInput
                    ref="searchInput"
                    v-model="searchQuery"
                    icon="magnifying-glass"
                    placeholder="Søg på OU-navn"
                />
            </SidebarHeader>
            <SidebarContent>
                <GroupSidebarBox
                    v-for="ou in filteredOUs"
                    :key="ou.distinguishedName"
                    :ou="ou"
                    :selected="selectedOU?.distinguishedName === ou.distinguishedName"
                    @click="toggleOUSelection(ou)"
                />
            </SidebarContent>
            <SidebarFooter>
                <GroupCreateGroup />
            </SidebarFooter>
        </Sidebar>
        <Workspace :filled="selectedOU != null">
            <section class="hero is-link" v-if="selectedOU">
                <div class="hero-body">
                    <p class="title is-3">{{ selectedOU.name }}</p>
                    <p class="subtitle is-6">{{ selectedOU.distinguishedName }}</p>
                </div>
            </section>
            <WorkspaceContent v-if="selectedOU">
                <div class="columns is-desktop">
                    <div class="column">
                        <GroupMetadata :selected-o-u="selectedOU" />
                    </div>
                    <div class="column"></div>
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
import GroupSidebarBox from '@/Components/Organisation/GroupSidebarBox.vue';
import GroupMetadata from '@/Components/Organisation/GroupMetadata.vue';
import GroupCreateGroup from '@/Components/Organisation/GroupCreateGroup.vue';

const userStore = useUserStore();
const { OUs: ous } = storeToRefs(userStore);

const selectedOU = ref<ADOU | null>(null);
const searchQuery = ref('');

const filteredOUs = computed(() => {
    const q = searchQuery.value.trim().toLowerCase();
    if (!q) return ous.value;
    return ous.value.filter((ou: ADOU) => ou.name.toLowerCase().includes(q));
});

const toggleOUSelection = (clicked: ADOU) => {
    if (selectedOU.value?.distinguishedName === clicked.distinguishedName) {
        selectedOU.value = null;
    } else {
        selectedOU.value = JSON.parse(JSON.stringify(clicked));
    }
};

// Ctrl+K search focus
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
    await userStore.GET_OUS();
});

onUnmounted(() => {
    window.removeEventListener('keydown', handleGlobalKeyDown);
});
</script>
