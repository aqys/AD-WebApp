<template>
    <div class="ou-sidebar-box" :class="{ active: selected }">
        <div class="ou-sidebar-box-name">
            <font-awesome-icon icon="sitemap" class="ou-icon" />
            <span>{{ ou.name }}</span>
        </div>
        <div class="ou-sidebar-box-dn">{{ ou.distinguishedName }}</div>
    </div>
</template>
<script lang="ts" setup>
defineProps<{
    ou: ADOU;
    selected: boolean;
}>();
</script>
<style lang="scss">
.ou-sidebar-box {
    border: 1px solid rgba(0, 0, 0, 0.1);
    padding: 0.9rem 1.2rem;
    border-radius: 10px;
    margin-bottom: 0.75rem;
    cursor: pointer;
    transition: background 0.15s, border-color 0.15s;

    &-name {
        font-weight: 600;
        font-size: 0.95rem;
        margin-bottom: 0.25rem;
        display: flex;
        align-items: center;
        gap: 0.4rem;
    }

    .ou-icon {
        color: #64748b;
        font-size: 0.85rem;
    }

    &-dn {
        font-size: 0.75rem;
        color: #94a3b8;
        word-break: break-all;
    }

    &.active {
        background: #ececec;
        border-color: #696969;
        box-shadow: 0 2px 6px rgba(0, 0, 0, 0.06);
    }

    &:hover:not(.active) {
        background: #f8fafc;
    }
}
</style>
