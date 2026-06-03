<template>
    <nav class="panel">
        <div class="panel-heading sg-members-heading">
            <span>Medlemmer</span>
            <div class="flex" />
            <BButton type="is-small is-warning" @click="addOpen = true">
                Tilføj medlem
            </BButton>
        </div>

        <!-- Member list -->
        <div v-if="loading" class="panel-block sg-loading">
            <span class="has-text-grey">Henter medlemmer…</span>
        </div>
        <template v-else-if="members.length > 0">
            <div
                v-for="memberDn in members"
                :key="memberDn"
                class="panel-block sg-member-row"
            >
                <span class="panel-icon">
                    <font-awesome-icon icon="user" />
                </span>
                <span class="sg-member-dn">{{ cnFromDn(memberDn) }}</span>
                <div class="flex" />
                <BButton
                    type="is-small is-danger"
                    :loading="removingDn === memberDn"
                    @click="removeMember(memberDn)"
                >
                    Fjern
                </BButton>
            </div>
        </template>
        <div v-else class="panel-block sg-empty">
            <span class="has-text-grey">Ingen medlemmer i gruppen</span>
        </div>
    </nav>

    <!-- Add member modal -->
    <BModal v-model="addOpen" has-modal-card>
        <div class="modal-card">
            <header class="modal-card-head">
                <p class="modal-card-title">Tilføj bruger til gruppe</p>
                <button type="button" class="delete" @click="addOpen = false" />
            </header>
            <section class="modal-card-body">
                <BField label="Vælg bruger">
                    <BSelect expanded v-model="selectedUserDn">
                        <option
                            v-for="user in users"
                            :key="user.distinguishedName"
                            :value="user.distinguishedName"
                        >
                            {{ user.username }}
                            <template v-if="user.firstName || user.lastName">
                                – {{ user.firstName }} {{ user.lastName }}
                            </template>
                        </option>
                    </BSelect>
                </BField>
            </section>
            <footer class="modal-card-foot">
                <BButton type="is-primary" :loading="adding" @click="addMember">Tilføj</BButton>
                <BButton @click="addOpen = false">Annuller</BButton>
            </footer>
        </div>
    </BModal>
</template>
<script lang="ts" setup>
import { ref, watch, onMounted } from 'vue';
import { BButton, BModal, BField, BSelect, useToast } from 'buefy';
import { useUserStore } from '@/Stores/UserStore';
import { storeToRefs } from 'pinia';

const props = defineProps<{ selectedGroup: ADSecurityGroup }>();

const Toast = useToast();
const userStore = useUserStore();
const { Users: users } = storeToRefs(userStore);

const members = ref<string[]>([]);
const loading = ref(false);
const adding = ref(false);
const addOpen = ref(false);
const removingDn = ref<string | null>(null);
const selectedUserDn = ref('');

const cnFromDn = (dn: string) => {
    const match = dn.match(/^CN=([^,]+)/i);
    return match ? match[1] : dn;
};

const loadMembers = async () => {
    loading.value = true;
    try {
        members.value = await userStore.GET_GROUP_MEMBERS(props.selectedGroup.distinguishedName);
    } finally {
        loading.value = false;
    }
};

watch(() => props.selectedGroup.distinguishedName, loadMembers, { immediate: true });
onMounted(() => userStore.GET_USERS());

const addMember = async () => {
    if (!selectedUserDn.value) {
        Toast.open({ message: 'Vælg en bruger', type: 'is-warning' });
        return;
    }
    adding.value = true;
    try {
        const ok = await userStore.ADD_MEMBER_TO_GROUP(
            props.selectedGroup.distinguishedName,
            selectedUserDn.value
        );
        Toast.open({
            message: ok ? 'Bruger tilføjet' : 'Kunne ikke tilføje bruger',
            type: ok ? 'is-success' : 'is-danger'
        });
        if (ok) {
            addOpen.value = false;
            selectedUserDn.value = '';
            await loadMembers();
        }
    } finally {
        adding.value = false;
    }
};

const removeMember = async (memberDn: string) => {
    removingDn.value = memberDn;
    try {
        const ok = await userStore.REMOVE_MEMBER_FROM_GROUP(
            props.selectedGroup.distinguishedName,
            memberDn
        );
        Toast.open({
            message: ok ? 'Bruger fjernet' : 'Kunne ikke fjerne bruger',
            type: ok ? 'is-success' : 'is-danger'
        });
        if (ok) await loadMembers();
    } finally {
        removingDn.value = null;
    }
};
</script>
<style lang="scss">
.sg-members-heading {
    padding: 15px 25px;
    min-height: 60px;
    align-items: center;
    display: flex;
    gap: 0.5rem;
}

.sg-member-row {
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.sg-member-dn {
    font-size: 0.85rem;
    word-break: break-all;
}

.sg-empty,
.sg-loading {
    color: #94a3b8;
    font-style: italic;
}

.flex {
    flex: 1;
}

.modal-card-foot {
    gap: 0.5rem;
    height: 3rem;
}
.modal-card-body {
    font-size: 1.05rem;
    height: 9rem;
}
</style>
