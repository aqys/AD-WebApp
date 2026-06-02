<template>
    <nav class="panel">
        <p class="panel-heading">Bruger metadata</p>
        <div class="panel-body metadata-body">

            <!-- ── Display name ──────────────────── -->
            <p class="metadata-section-title">Navn</p>
            <BField label="Fornavn">
                <BInput v-model="editFirstName" />
            </BField>
            <BField label="Efternavn">
                <BInput v-model="editLastName" />
            </BField>
            <BButton
                type="is-primary"
                size="is-small"
                :loading="savingName"
                @click="saveDisplayName"
            >
                Gem navn
            </BButton>

            <hr />

            <!-- ── Change password ───────────────── -->
            <p class="metadata-section-title">Adgangskode</p>
            <BField label="Ny adgangskode">
                <BInput v-model="newPassword" type="password" password-reveal />
            </BField>
            <BButton
                type="is-warning"
                size="is-small"
                :loading="savingPassword"
                @click="savePassword"
            >
                Skift adgangskode
            </BButton>

            <hr />

            <!-- ── Disable user ──────────────────── -->
            <p class="metadata-section-title">Konto</p>
            <div class="user-status-row">
                <span class="tag" :class="selectedUser.isEnabled ? 'is-success' : 'is-danger'">
                    {{ selectedUser.isEnabled ? 'Aktiv' : 'Deaktiveret' }}
                </span>
                <BButton
                    v-if="selectedUser.isEnabled"
                    type="is-danger"
                    size="is-small"
                    :loading="disabling"
                    @click="confirmDisable = true"
                >
                    Deaktiver bruger
                </BButton>
            </div>
        </div>
    </nav>

    <!-- Confirm disable modal -->
    <BModal v-model="confirmDisable" has-modal-card>
        <div class="modal-card">
            <header class="modal-card-head">
                <p class="modal-card-title">Deaktiver bruger?</p>
            </header>
            <section class="modal-card-body">
                Er du sikker på at du vil deaktivere <strong>{{ selectedUser.username }}</strong>?
                Brugeren vil ikke kunne logge ind.
            </section>
            <footer class="modal-card-foot">
                <BButton type="is-danger" :loading="disabling" @click="disableUser">Deaktiver</BButton>
                <BButton @click="confirmDisable = false">Annuller</BButton>
            </footer>
        </div>
    </BModal>
</template>
<script lang="ts" setup>
import { ref, watch } from 'vue';
import { BField, BInput, BButton, BModal, useToast } from 'buefy';
import { useUserStore } from '@/Stores/UserStore';

const props = defineProps<{ selectedUser: ADUser }>();
const emit = defineEmits<{ (e: 'updated'): void }>();

const Toast = useToast();
const userStore = useUserStore();

const editFirstName = ref(props.selectedUser.firstName);
const editLastName = ref(props.selectedUser.lastName);
const newPassword = ref('');
const savingName = ref(false);
const savingPassword = ref(false);
const disabling = ref(false);
const confirmDisable = ref(false);

// Sync fields when a different user is selected
watch(() => props.selectedUser, (u) => {
    editFirstName.value = u.firstName;
    editLastName.value = u.lastName;
    newPassword.value = '';
});

const saveDisplayName = async () => {
    savingName.value = true;
    try {
        const ok = await userStore.CHANGE_DISPLAY_NAME(
            props.selectedUser.username,
            editFirstName.value,
            editLastName.value
        );
        Toast.open({ message: ok ? 'Navn gemt' : 'Kunne ikke gemme navn', type: ok ? 'is-success' : 'is-danger' });
        if (ok) emit('updated');
    } finally {
        savingName.value = false;
    }
};

const savePassword = async () => {
    if (!newPassword.value) {
        Toast.open({ message: 'Angiv en ny adgangskode', type: 'is-warning' });
        return;
    }
    savingPassword.value = true;
    try {
        const ok = await userStore.CHANGE_PASSWORD(props.selectedUser.username, newPassword.value);
        Toast.open({ message: ok ? 'Adgangskode ændret' : 'Kunne ikke ændre adgangskode', type: ok ? 'is-success' : 'is-danger' });
        if (ok) newPassword.value = '';
    } finally {
        savingPassword.value = false;
    }
};

const disableUser = async () => {
    disabling.value = true;
    try {
        const ok = await userStore.DISABLE_USER(props.selectedUser.username);
        Toast.open({ message: ok ? 'Bruger deaktiveret' : 'Kunne ikke deaktivere bruger', type: ok ? 'is-success' : 'is-danger' });
        confirmDisable.value = false;
        if (ok) emit('updated');
    } finally {
        disabling.value = false;
    }
};
</script>
<style lang="scss">
.metadata-body {
    padding: 1.2rem;
}

.metadata-section-title {
    font-size: 0.75rem;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    color: #94a3b8;
    margin-bottom: 0.75rem;
}

.user-status-row {
    display: flex;
    align-items: center;
    gap: 1rem;
}

.modal-card-foot {
    gap: 0.5rem;
    height: 2rem;
}
.modal-card-body {
    font-size: 1.05rem;
    height: 1rem;
}
</style>