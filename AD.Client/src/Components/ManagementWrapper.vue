<template>
    <div class="management-wrapper">
        <slot />
    </div>
</template>
<style lang="scss">
.management-wrapper {
    display: flex;
    flex: 1;
    width: 100%;
    padding: 1rem;
    padding-top: 0;
    gap: 1rem;
    min-height: 0;
    scrollbar-width: thin;
}
</style>