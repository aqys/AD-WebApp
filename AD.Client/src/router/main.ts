import { createRouter, createWebHistory, type RouteRecordRaw } from "vue-router";

const routes: RouteRecordRaw[] = [
    {
        path: "/user",
        component: () => import("../Views/User.vue")
    },
    {
        path: "/organisation",
        component: () => import("../Views/Organisation.vue")
    },
    {
        path: "/security-groups",
        component: () => import("../Views/SecurityGroups.vue")
    }
];

const router = createRouter({
    history: createWebHistory(),
    routes
});

export default router;