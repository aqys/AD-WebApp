<template>
    <nav class="panel">
        <p class="panel-heading">Bruger metadata</p>
        <div class="panel-body metadata-body">

            
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
                <BButton
                    v-else
                    type="is-success"
                    size="is-small"
                    :loading="enabling"
                    @click="confirmEnable = true"
                >
                    Aktiver bruger
                </BButton>
            </div>
        </div>
    </nav>

    
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

    
    <BModal v-model="confirmEnable" has-modal-card>
        <div class="modal-card">
            <header class="modal-card-head">
                <p class="modal-card-title">Aktiver bruger?</p>
            </header>
            <section class="modal-card-body">
                Er du sikker på at du vil aktivere <strong>{{ selectedUser.username }}</strong>?
            </section>
            <footer class="modal-card-foot">
                <BButton type="is-success" :loading="enabling" @click="enableUser">Aktiver</BButton>
                <BButton @click="confirmEnable = false">Annuller</BButton>
            </footer>
        </div>
    </BModal>
</template>
<script lang="ts" setup>
import { ref, watch } from 'vue';
import { BField, BInput, BButton, BModal, useToast } from 'buefy';
import { useUserStore } from '@/Stores/UserStore';
import { validatePassword } from '@/Utils/validation';

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
const enabling = ref(false);
const confirmEnable = ref(false);


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
    const validationError = validatePassword(
        newPassword.value,
        props.selectedUser.username,
        props.selectedUser.firstName,
        props.selectedUser.lastName
    );
    if (validationError) {
        Toast.open({ 
            message: validationError, 
            type: 'is-danger',
            duration: 6000,
            queue: false 
        });
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

const enableUser = async () => {
    enabling.value = true;
    try {
        const ok = await userStore.ENABLE_USER(props.selectedUser.username);
        Toast.open({ message: ok ? 'Bruger aktiveret' : 'Kunne ikke aktivere bruger', type: ok ? 'is-success' : 'is-danger' });
        confirmEnable.value = false;
        if (ok) emit('updated');
    } finally {
        enabling.value = false;
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