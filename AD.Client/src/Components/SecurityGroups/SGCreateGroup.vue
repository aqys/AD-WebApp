<template>
    <BButton type="is-primary" @click="openCreate" icon-left="plus">
        Opret sikkerhedsgruppe
    </BButton>
    <BModal v-model="open" has-modal-card>
        <div class="modal-card">
            <header class="modal-card-head">
                <p class="modal-card-title">Opret sikkerhedsgruppe</p>
                <button type="button" class="delete" @click="open = false" />
            </header>
            <section class="modal-card-body" v-if="form">
                <BField label="Gruppenavn">
                    <BInput v-model="form.groupName" placeholder="F.eks. Drev-Salg" />
                </BField>
                <BField label="Placering (OU)">
                    <BSelect expanded v-model="form.parentPath">
                        <option
                            v-for="ou in ous"
                            :key="ou.distinguishedName"
                            :value="ou.distinguishedName"
                        >
                            {{ ou.name }}
                        </option>
                    </BSelect>
                </BField>
            </section>
            <footer class="modal-card-foot">
                <BButton type="is-primary" :loading="loading" @click="createGroup">
                    Opret gruppe
                </BButton>
                <BButton @click="open = false">Annuller</BButton>
            </footer>
        </div>
    </BModal>
</template>
<script setup lang="ts">
import { useUserStore } from '@/Stores/UserStore';
import { BModal, BField, BInput, BSelect, BButton, useToast } from 'buefy';
import { ref, onMounted } from 'vue';
import { storeToRefs } from 'pinia';

const Toast = useToast();
const userStore = useUserStore();
const { OUs: ous } = storeToRefs(userStore);

const form = ref<{ groupName: string; parentPath: string } | null>(null);
const open = ref(false);
const loading = ref(false);

onMounted(async () => {
    if (ous.value.length === 0) {
        await userStore.GET_OUS();
    }
});

const openCreate = () => {
    form.value = { groupName: '', parentPath: ous.value[0]?.distinguishedName ?? '' };
    open.value = true;
};

const createGroup = async () => {
    if (!form.value) return;
    const { groupName, parentPath } = form.value;

    if (!groupName || !parentPath) {
        Toast.open({ message: 'Udfyld alle felter', type: 'is-warning' });
        return;
    }

    loading.value = true;
    try {
        const success = await userStore.CREATE_SECURITY_GROUP(groupName, parentPath);
        if (success) {
            Toast.open({ message: `Gruppe '${groupName}' oprettet`, type: 'is-success' });
            open.value = false;
            form.value = null;
        } else {
            Toast.open({ message: 'Kunne ikke oprette gruppen', type: 'is-danger' });
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
    height: 14rem;
}
</style>

