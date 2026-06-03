import UserService from "@/Services/UserService";
import SecurityGroupService from "@/Services/SecurityGroupService";
import { defineStore } from "pinia";
import { computed, ref } from "vue";


function ouNameFromDN(dn: string): string {
    
    const match = dn.match(/^OU=([^,]+)/i);
    return match ? match[1] : dn;
}

export const useUserStore = defineStore("user", () => {
    const userService = new UserService();
    const securityGroupService = new SecurityGroupService();

    const Users = ref<ADUser[]>([]);
    const OUs = ref<ADOU[]>([]);
    const SecurityGroups = ref<ADSecurityGroup[]>([]);

    const USERS = computed(() => Users.value);
    const OUS = computed(() => OUs.value);
    const SECURITY_GROUPS = computed(() => SecurityGroups.value);

    

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

    async function GET_SECURITY_GROUPS() {
        const data = await securityGroupService.getGroups();
        SecurityGroups.value = data;
        return data;
    }

    async function CREATE_SECURITY_GROUP(groupName: string, parentPath: string) {
        const success = await securityGroupService.createGroup(groupName, parentPath);
        if (success) await GET_SECURITY_GROUPS();
        return success;
    }

    async function DELETE_SECURITY_GROUP(distinguishedName: string) {
        const success = await securityGroupService.deleteGroup(distinguishedName);
        if (success) await GET_SECURITY_GROUPS();
        return success;
    }

    async function GET_GROUP_MEMBERS(groupDn: string) {
        return await securityGroupService.getMembers(groupDn);
    }

    async function ADD_MEMBER_TO_GROUP(groupDn: string, userDn: string) {
        return await securityGroupService.addMember(groupDn, userDn);
    }

    async function REMOVE_MEMBER_FROM_GROUP(groupDn: string, userDn: string) {
        return await securityGroupService.removeMember(groupDn, userDn);
    }

    return {
        Users, OUs, SecurityGroups,
        USERS, OUS, SECURITY_GROUPS,
        GET_USERS, CREATE_USER, DISABLE_USER, ENABLE_USER, CHANGE_PASSWORD, CHANGE_DISPLAY_NAME,
        GET_OUS, CREATE_OU, ASSIGN_USER_TO_OU, REMOVE_USER_FROM_OU,
        GET_SECURITY_GROUPS, CREATE_SECURITY_GROUP, DELETE_SECURITY_GROUP,
        GET_GROUP_MEMBERS, ADD_MEMBER_TO_GROUP, REMOVE_MEMBER_FROM_GROUP,
    };
});