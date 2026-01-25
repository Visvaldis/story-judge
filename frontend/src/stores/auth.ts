import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import type { User } from '../types';
import { authService } from '../services/auth';
import type { RegisterRequest, LoginRequest } from '../services/auth';

export const useAuthStore = defineStore('auth', () => {
  const user = ref<User | null>(null);
  const loading = ref(false);
  const error = ref<string | null>(null);

  const isAuthenticated = computed(() => !!user.value);
  const isAdmin = computed(() => user.value?.roleLevel === 'Admin');
  const isModerator = computed(() => user.value?.roleLevel === 'Admin' || user.value?.roleLevel === 'Moderator');

  async function register(data: RegisterRequest) {
    loading.value = true;
    error.value = null;

    try {
      const response = await authService.register(data);
      authService.setToken(response.token);
      user.value = response.user;
      return true;
    } catch (err: any) {
      error.value = err.response?.data?.message || 'Registration failed';
      return false;
    } finally {
      loading.value = false;
    }
  }

  async function login(data: LoginRequest) {
    loading.value = true;
    error.value = null;

    try {
      const response = await authService.login(data);
      authService.setToken(response.token);
      user.value = response.user;
      return true;
    } catch (err: any) {
      error.value = err.response?.data?.message || 'Login failed';
      return false;
    } finally {
      loading.value = false;
    }
  }

  async function fetchCurrentUser() {
    if (!authService.isAuthenticated()) {
      return;
    }

    loading.value = true;
    error.value = null;

    try {
      user.value = await authService.getCurrentUser();
    } catch (err: any) {
      error.value = err.response?.data?.error || 'Failed to fetch user';
      authService.removeToken();
      user.value = null;
    } finally {
      loading.value = false;
    }
  }

  function setToken(token: string) {
    authService.setToken(token);
  }

  function logout() {
    authService.removeToken();
    user.value = null;
  }

  function getGoogleLoginUrl() {
    return authService.getGoogleLoginUrl();
  }

  function getLinkedInLoginUrl() {
    return authService.getLinkedInLoginUrl();
  }

  return {
    user,
    loading,
    error,
    isAuthenticated,
    isAdmin,
    isModerator,
    register,
    login,
    fetchCurrentUser,
    setToken,
    logout,
    getGoogleLoginUrl,
    getLinkedInLoginUrl
  };
});
