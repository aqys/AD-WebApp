<template>
    <BButton type="is-primary" @click="openCreate" icon-left="plus">
        Opret ny OU
    </BButton>
    <BModal v-model="open" has-modal-card>
        <div class="modal-card">
            <header class="modal-card-head">
                <p class="modal-card-title">Opret organisationsenhed (OU)</p>
                <button type="button" class="delete" @click="open = false" />
            </header>
            <section class="modal-card-body" v-if="form">
                <BField label="OU-navn">
                    <BInput v-model="form.ouName" placeholder="F.eks. Salg" />
                </BField>
                <BField label="Overordnet OU (Distinguished Name)">
                    <BInput v-model="form.parentPath" placeholder="F.eks. DC=corp,DC=local" />
                </BField>
            </section>
            <footer class="modal-card-foot">
                <BButton type="is-primary" :loading="loading" @click="createOU">
                    Opret OU
                </BButton>
                <BButton @click="open = false">Annuller</BButton>
            </footer>
        </div>
    </BModal>
</template>
<script setup lang="ts">
import { useUserStore } from '@/Stores/UserStore';
import { BModal, BField, BInput, BButton, useToast } from 'buefy';
import { ref } from 'vue';

const Toast = useToast();
const userStore = useUserStore();

const form = ref<{ ouName: string; parentPath: string } | null>(null);
const open = ref(false);
const loading = ref(false);

const openCreate = () => {
    form.value = { ouName: '', parentPath: '' };
    open.value = true;
};

const createOU = async () => {
    if (!form.value) return;
    const { ouName, parentPath } = form.value;

    if (!ouName || !parentPath) {
        Toast.open({ message: 'Udfyld alle felter', type: 'is-warning' });
        return;
    }

    loading.value = true;
    try {
        const success = await userStore.CREATE_OU(ouName, parentPath);
        if (success) {
            Toast.open({ message: `OU '${ouName}' oprettet`, type: 'is-success' });
            open.value = false;
            form.value = null;
        } else {
            Toast.open({ message: 'Kunne ikke oprette OU', type: 'is-danger' });
        }
    } finally {
        loading.value = false;
    }
};
</script>
