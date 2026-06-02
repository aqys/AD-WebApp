import UserService from "@/Services/UserService";
import { defineStore } from "pinia";
import { computed, ref } from "vue";


function ouNameFromDN(dn: string): string {
    
    const match = dn.match(/^OU=([^,]+)/i);
    return match ? match[1] : dn;
}

export const useUserStore = defineStore("user", () => {
    const userService = new UserService();

    const Users = ref<ADUser[]>([]);
    const OUs = ref<ADOU[]>([]);

    const USERS = computed(() => Users.value);
    const OUS = computed(() => OUs.value);

    

    async function GET_USERS() {
        const data = await userService.getUsers();
        Users.value = data;
        return data;
    }

    async function CREATE_USER(username: string, firstName: string, lastName: string, password: string) {
        const success = await userService.createUser({ username, firstName, lastName, password });
        if (success) await GET_USERS();
        return success;
    }

    async function DISABLE_USER(username: string) {
        const success = await userService.disableUser(username);
        if (success) await GET_USERS();
        return success;
    }

    async function ENABLE_USER(username: string) {
        const success = await userService.enableUser(username);
        if (success) await GET_USERS();
        return success;
    }

    async function CHANGE_PASSWORD(username: string, newPassword: string) {
        return await userService.changePassword(username, newPassword);
    }

    async function CHANGE_DISPLAY_NAME(username: string, firstName: string, lastName: string) {
        const success = await userService.changeDisplayName(username, firstName, lastName);
        if (success) await GET_USERS();
        return success;
    }

    

    async function GET_OUS() {
        const data = await userService.getOUs();
        OUs.value = data.map((dn: string) => ({ distinguishedName: dn, name: ouNameFromDN(dn) }));
        return OUs.value;
    }

    async function CREATE_OU(ouName: string, parentPath: string) {
        const success = await userService.createOU(ouName, parentPath);
        if (success) await GET_OUS();
        return success;
    }

    async function ASSIGN_USER_TO_OU(username: string, ouPath: string) {
        const success = await userService.assignUserToOU(username, ouPath);
        if (success) await GET_USERS();
        return success;
    }

    async function REMOVE_USER_FROM_OU(username: string) {
        const success = await userService.removeUserFromOU(username);
        if (success) await GET_USERS();
        return success;
    }

    return {
        Users, OUs,
        USERS, OUS,
        GET_USERS, CREATE_USER, DISABLE_USER, ENABLE_USER, CHANGE_PASSWORD, CHANGE_DISPLAY_NAME,
        GET_OUS, CREATE_OU, ASSIGN_USER_TO_OU, REMOVE_USER_FROM_OU,
    };
});