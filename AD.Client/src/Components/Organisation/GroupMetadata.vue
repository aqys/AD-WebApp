<template>
    <nav class="panel">
        <p class="panel-heading">OU metadata</p>
        <div class="panel-body ou-metadata-body">
            <BField label="Navn">
                <BInput :model-value="ouName" disabled />
            </BField>
            <BField label="Distinguished Name">
                <BInput :model-value="selectedOU.distinguishedName" disabled />
            </BField>
        </div>
    </nav>
</template>
<script lang="ts" setup>
import { computed } from 'vue';
import { BField, BInput } from 'buefy';

const props = defineProps<{ selectedOU: ADOU }>();

const ouName = computed(() => {
    const match = props.selectedOU.distinguishedName.match(/^OU=([^,]+)/i);
    return match ? match[1] : props.selectedOU.distinguishedName;
});
</script>
<style lang="scss">
.ou-metadata-body {
    padding: 1rem;
}
</style>
