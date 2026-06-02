<template>
    <Back icon="user-pen" title="Brugerstyring" />
    <ManagementWrapper>
        <Sidebar>
            <SidebarHeader>
                <BInput
                    class="custom-search-input"
                    ref="searchInput"
                    v-model="searchQuery"
                    icon="magnifying-glass"
                    placeholder="Søg på navn eller brugernavn"
                />
            </SidebarHeader>
            <SidebarContent>
                <UserSidebarBox
                    v-for="user in filteredUsers"
                    :key="user.username"
                    :user="user"
                    :selected="selectedUser?.username === user.username"
                    @click="toggleUserSelection(user)"
                />
            </SidebarContent>
            <SidebarFooter>
                <UserCreateUser />
            </SidebarFooter>
        </Sidebar>
        <Workspace :filled="selectedUser != null">
            <section class="hero is-link" v-if="selectedUser">
                <div class="hero-body">
                    <p class="title is-3">{{ selectedUser.firstName }} {{ selectedUser.lastName }}</p>
                    <p class="subtitle is-6">{{ selectedUser.username }}</p>
                </div>
            </section>
            <WorkspaceContent v-if="selectedUser">
                <div class="columns is-desktop">
                    <div class="column">
                        <UserMetadata :selected-user="selectedUser" @updated="onUserUpdated" />
                    </div>
                    <div class="column">
                        <UserOU :selected-user="selectedUser" @updated="onUserUpdated" />
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
import UserSidebarBox from '@/Components/User/UserSidebarBox.vue';
import UserCreateUser from '@/Components/User/UserCreateUser.vue';
import UserMetadata from '@/Components/User/UserMetadata.vue';
import UserOU from '@/Components/User/UserOU.vue';

const userStore = useUserStore();
const { Users: users } = storeToRefs(userStore);

const selectedUser = ref<ADUser | null>(null);
const searchQuery = ref('');

const filteredUsers = computed(() => {
    const q = searchQuery.value.trim().toLowerCase();
    if (!q) return users.value;
    return users.value.filter((u: ADUser) =>
        u.username.toLowerCase().includes(q) ||
        u.firstName.toLowerCase().includes(q) ||
        u.lastName.toLowerCase().includes(q)
    );
});

const toggleUserSelection = (clicked: ADUser) => {
    if (selectedUser.value?.username === clicked.username) {
        selectedUser.value = null;
    } else {
        selectedUser.value = JSON.parse(JSON.stringify(clicked));
    }
};

// After an update, refresh and re-sync the selected user object
const onUserUpdated = async () => {
    await userStore.GET_USERS();
    if (selectedUser.value) {
        const refreshed = users.value.find((u: ADUser) => u.username === selectedUser.value!.username);
        selectedUser.value = refreshed ? JSON.parse(JSON.stringify(refreshed)) : null;
    }
};

// Ctrl+K focuses the search box
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
    await userStore.GET_USERS();
    await userStore.GET_OUS();
});

onUnmounted(() => {
    window.removeEventListener('keydown', handleGlobalKeyDown);
});
</script>
<style lang="scss">
.custom-search-input {
    input {
        background-color: #171a1f;
        color: #e2e8f0;
        border: none;
        border-radius: 6px;
        box-shadow: none;
        
        &::placeholder {
            color: #475569;
            font-weight: 500;
        }
        
        &:focus {
            box-shadow: inset 0 0 0 1px #334155;
            background-color: #1e2228;
        }
    }
    
    .icon {
        color: #475569 !important;
    }
}
</style>
