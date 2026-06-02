<template>
    <nav class="panel">
        <div class="panel-heading ou-panel-heading">
            <span>Organisationsenhed (OU)</span>
            <div class="flex" />
            <BButton type="is-small is-warning" @click="assignOpen = true" :disabled="ous.length === 0">
                Tildel OU
            </BButton>
        </div>

        <!-- Current OU -->
        <div class="panel-block ou-current" v-if="selectedUser.ouPath">
            <span class="panel-icon">
                <font-awesome-icon icon="sitemap" />
            </span>
            <span class="ou-dn">{{ currentOUName }}</span>
            <div class="flex" />
            <BButton type="is-small is-danger" @click="confirmRemoveOpen = true">
                Fjern
            </BButton>
        </div>
        <div class="panel-block ou-empty" v-else>
            <span class="has-text-grey">Ingen OU tildelt</span>
        </div>
    </nav>

    <!-- Assign OU modal -->
    <BModal v-model="assignOpen" has-modal-card>
        <div class="modal-card">
            <header class="modal-card-head">
                <p class="modal-card-title">Tildel OU til bruger</p>
                <button type="button" class="delete" @click="assignOpen = false" />
            </header>
            <section class="modal-card-body">
                <BField label="Vælg OU">
                    <BSelect expanded v-model="selectedOU">
                        <option v-for="ou in ous" :key="ou.distinguishedName" :value="ou.distinguishedName">
                            {{ ou.name }}
                        </option>
                    </BSelect>
                </BField>
            </section>
            <footer class="modal-card-foot">
                <BButton type="is-primary" :loading="assigning" @click="assignOU">Tildel</BButton>
                <BButton @click="assignOpen = false">Annuller</BButton>
            </footer>
        </div>
    </BModal>

    <!-- Confirm remove modal -->
    <BModal v-model="confirmRemoveOpen" has-modal-card>
        <div class="modal-card">
            <header class="modal-card-head">
                <p class="modal-card-title">Fjern fra OU?</p>
            </header>
            <section class="modal-card-body">
                Fjern <strong>{{ selectedUser.username }}</strong> fra OU og flyt til standard-container?
            </section>
            <footer class="modal-card-foot">
                <BButton type="is-danger" :loading="removing" @click="removeOU">Fjern</BButton>
                <BButton @click="confirmRemoveOpen = false">Annuller</BButton>
            </footer>
        </div>
    </BModal>
</template>
<script lang="ts" setup>
import { ref, computed } from 'vue';
import { BButton, BField, BModal, BSelect, useToast } from 'buefy';
import { useUserStore } from '@/Stores/UserStore';
import { storeToRefs } from 'pinia';

const props = defineProps<{ selectedUser: ADUser }>();
const emit = defineEmits<{ (e: 'updated'): void }>();

const Toast = useToast();
const userStore = useUserStore();
const { OUs: ous } = storeToRefs(userStore);

const assignOpen = ref(false);
const confirmRemoveOpen = ref(false);
const assigning = ref(false);
const removing = ref(false);
const selectedOU = ref<string>('');

const currentOUName = computed(() => {
    if (!props.selectedUser.ouPath) return '';
    const match = props.selectedUser.ouPath.match(/^OU=([^,]+)/i);
    return match ? match[1] : props.selectedUser.ouPath;
});

const assignOU = async () => {
    if (!selectedOU.value) {
        Toast.open({ message: 'Vælg en OU', type: 'is-warning' });
        return;
    }
    assigning.value = true;
    try {
        const ok = await userStore.ASSIGN_USER_TO_OU(props.selectedUser.username, selectedOU.value);
        Toast.open({ message: ok ? 'OU tildelt' : 'Kunne ikke tildele OU', type: ok ? 'is-success' : 'is-danger' });
        if (ok) { assignOpen.value = false; emit('updated'); }
    } finally {
        assigning.value = false;
    }
};

const removeOU = async () => {
    removing.value = true;
    try {
        const ok = await userStore.REMOVE_USER_FROM_OU(props.selectedUser.username);
        Toast.open({ message: ok ? 'Fjernet fra OU' : 'Kunne ikke fjerne fra OU', type: ok ? 'is-success' : 'is-danger' });
        confirmRemoveOpen.value = false;
        if (ok) emit('updated');
    } finally {
        removing.value = false;
    }
};
</script>
<style lang="scss">
.ou-panel-heading {
    padding: 15px 25px;
    min-height: 60px;
    align-items: center;
    display: flex;
    gap: 0.5rem;
}

.ou-current {
    display: flex;
    align-items: center;
    gap: 0.5rem;
}

.ou-dn {
    font-size: 0.85rem;
    word-break: break-all;
}

.ou-empty {
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
    height: 8rem;
}
</style>
