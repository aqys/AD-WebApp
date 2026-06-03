<template>
    <nav class="panel">
        <p class="panel-heading">Gruppe metadata</p>
        <div class="panel-body sg-metadata-body">
            <p class="sg-section-title">Navn</p>
            <BField label="Gruppenavn">
                <BInput :model-value="selectedGroup.name" disabled />
            </BField>
            <BField label="Distinguished Name">
                <BInput :model-value="selectedGroup.distinguishedName" disabled />
            </BField>

            <hr />

            <p class="sg-section-title">Slet gruppe</p>
            <BButton
                type="is-danger"
                size="is-small"
                :loading="deleting"
                @click="confirmDeleteOpen = true"
            >
                Slet gruppe
            </BButton>
        </div>
    </nav>

    <!-- Confirm delete modal -->
    <BModal v-model="confirmDeleteOpen" has-modal-card>
        <div class="modal-card">
            <header class="modal-card-head">
                <p class="modal-card-title">Slet gruppe?</p>
            </header>
            <section class="modal-card-body">
                Er du sikker på at du vil slette gruppen
                <strong>{{ selectedGroup.name }}</strong>?
                Denne handling kan ikke fortrydes.
            </section>
            <footer class="modal-card-foot">
                <BButton type="is-danger" :loading="deleting" @click="deleteGroup">Slet</BButton>
                <BButton @click="confirmDeleteOpen = false">Annuller</BButton>
            </footer>
        </div>
    </BModal>
</template>
<script lang="ts" setup>
import { ref } from 'vue';
import { BField, BInput, BButton, BModal, useToast } from 'buefy';
import { useUserStore } from '@/Stores/UserStore';

const props = defineProps<{ selectedGroup: ADSecurityGroup }>();
const emit = defineEmits<{ (e: 'deleted'): void }>();

const Toast = useToast();
const userStore = useUserStore();

const deleting = ref(false);
const confirmDeleteOpen = ref(false);

const deleteGroup = async () => {
    deleting.value = true;
    try {
        const ok = await userStore.DELETE_SECURITY_GROUP(props.selectedGroup.distinguishedName);
        Toast.open({
            message: ok ? 'Gruppe slettet' : 'Kunne ikke slette gruppen',
            type: ok ? 'is-success' : 'is-danger'
        });
        confirmDeleteOpen.value = false;
        if (ok) emit('deleted');
    } finally {
        deleting.value = false;
    }
};
</script>
<style lang="scss">
.sg-metadata-body {
    padding: 1.2rem;
}

.sg-section-title {
    font-size: 0.75rem;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    color: #94a3b8;
    margin-bottom: 0.75rem;
}

.modal-card-foot {
    gap: 0.5rem;
    height: 3rem;
}
.modal-card-body {
    font-size: 1.05rem;
    height: 6rem;
}
</style>
