import { createRouter, createWebHistory, type RouteRecordRaw } from "vue-router";

const routes: RouteRecordRaw[] = [
    {
        path: "/",
        component: () => import("../Views/Home.vue")
    },
    {
        path: "/user",
        component: () => import("../Views/User.vue")
    },
    {
        path: "/organisation",
        component: () => import("../Views/Organisation.vue")
    }
];

const router = createRouter({
    history: createWebHistory(),
    routes
});

export default router;