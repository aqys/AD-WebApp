<template>
    <div class="user-sidebar-box" :class="{ active: selected }">
        <div class="user-sidebar-box-name">
            <span> {{ user.username }} </span>
            <span v-if="!user.isEnabled" class="tag is-danger is-small ml-1">Deaktiveret</span>
        </div>
        <div class="user-sidebar-box-username" v-if="hasName">{{ user.username }}</div>
        <div class="user-sidebar-box-ou" v-if="user.ouPath">
            <font-awesome-icon icon="sitemap" class="ou-icon" />
            {{ ouDisplayName }}
        </div>
    </div>
</template>
<script lang="ts" setup>
import { computed } from 'vue';

const props = defineProps<{
    user: ADUser;
    selected: boolean;
}>();

const hasName = computed(() => {
    return !!(props.user.firstName || props.user.lastName);
});

const ouDisplayName = computed(() => {
    if (!props.user.ouPath) return '';
    const match = props.user.ouPath.match(/^OU=([^,]+)/i);
    return match ? match[1] : props.user.ouPath;
});
</script>
<style lang="scss">
.user-sidebar-box {
    border: 1px solid rgba(0, 0, 0, 0.1);
    padding: 0.9rem 1.2rem;
    border-radius: 10px;
    margin-bottom: 0.75rem;
    cursor: pointer;
    transition: background 0.15s, border-color 0.15s;

    &-name {
        font-weight: 600;
        font-size: 0.95rem;
        margin-bottom: 0.15rem;
        display: flex;
        align-items: center;
        gap: 0.3rem;
    }

    &-username {
        font-size: 0.8rem;
        color: #64748b;
        margin-bottom: 0.3rem;
    }

    &-ou {
        font-size: 0.78rem;
        color: #94a3b8;
        display: flex;
        align-items: center;
    }

    .ou-icon {
        font-size: 0.7rem;
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