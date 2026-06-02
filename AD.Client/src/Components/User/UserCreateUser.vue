<template>
    <BButton type="is-primary" @click="openCreate" icon-left="plus">
        Tilføj ny bruger
    </BButton>
    <BModal v-model="open" has-modal-card>
        <div class="modal-card">
            <header class="modal-card-head">
                <p class="modal-card-title">Opret ny bruger</p>
                <button type="button" class="delete" @click="open = false" />
            </header>
            <section class="modal-card-body" v-if="form">
                <BField label="Fornavn">
                    <BInput v-model="form.firstName" placeholder="Fornavn" />
                </BField>
                <BField label="Efternavn">
                    <BInput v-model="form.lastName" placeholder="Efternavn" />
                </BField>
                <BField label="Brugernavn">
                    <BInput v-model="form.username" placeholder="f.eks. jdoe" />
                </BField>
                <BField label="Adgangskode">
                    <BInput v-model="form.password" type="password" password-reveal placeholder="Midlertidig adgangskode" />
                </BField>
            </section>
            <footer class="modal-card-foot">
                <BButton type="is-primary" :loading="loading" @click="createUser">
                    Opret bruger
                </BButton>
                <BButton @click="open = false">Annuller</BButton>
            </footer>
        </div>
    </BModal>
</template>
<script setup lang="ts">
import { useUserStore } from '@/Stores/UserStore';
import { BModal, BField, BInput, BButton, useToast } from 'buefy';
import { ref, watch } from 'vue';
import { validatePassword } from '@/Utils/validation';

const Toast = useToast();
const userStore = useUserStore();

const open = ref(false);
const loading = ref(false);
const form = ref<{ firstName: string; lastName: string; username: string; password: string } | null>(null);

const openCreate = () => {
    form.value = { firstName: '', lastName: '', username: '', password: '' };
    open.value = true;
};


watch(
    () => [form.value?.firstName, form.value?.lastName],
    ([first, last]) => {
        if (!form.value) return;
        const f = (first ?? '').toLowerCase().replace(/\s+/g, '');
        const l = (last ?? '').toLowerCase().replace(/\s+/g, '');
        form.value.username = `${f}${l}`;
    }
);

const createUser = async () => {
    if (!form.value) return;
    const { username, firstName, lastName, password } = form.value;

    if (!username || !firstName || !lastName || !password) {
        Toast.open({ message: 'Udfyld alle felter', type: 'is-warning' });
        return;
    }

    const validationError = validatePassword(password, username, firstName, lastName);
    if (validationError) {
        Toast.open({ 
            message: validationError, 
            type: 'is-danger',
            duration: 6000,
            queue: false 
        });
        return;
    }

    loading.value = true;
    try {
        const success = await userStore.CREATE_USER(username, firstName, lastName, password);
        if (success) {
            Toast.open({ message: `Bruger ${username} oprettet`, type: 'is-success' });
            open.value = false;
            form.value = null;
        } else {
            Toast.open({ message: 'Kunne ikke oprette bruger', type: 'is-danger' });
        }
    } finally {
        loading.value = false;
    }
};
</script>

<style lang="scss" scoped>
.modal-card-foot {
    gap: 0.5rem;
    height: 3rem;
}
.modal-card-body {
    font-size: 1.05rem;
    height: 25rem;
}
</style>