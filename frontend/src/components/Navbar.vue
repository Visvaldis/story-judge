<script setup lang="ts">
import { useAuthStore } from '../stores/auth';
import { useThemeStore } from '../stores/theme';
import { useRouter } from 'vue-router';

const authStore = useAuthStore();
const themeStore = useThemeStore();
const router = useRouter();

function handleLogout() {
  authStore.logout();
  router.push('/');
}
</script>

<template>
  <nav class="navbar">
    <div class="navbar-brand">
      <router-link to="/" class="brand-link">StoryJudge</router-link>
    </div>

    <div class="navbar-menu">
      <router-link to="/explore" class="nav-link">Explore</router-link>

      <button class="theme-toggle" @click="themeStore.toggleTheme" :title="themeStore.theme === 'dark' ? 'Switch to light mode' : 'Switch to dark mode'">
        <svg v-if="themeStore.theme === 'dark'" viewBox="0 0 24 24" width="20" height="20" fill="currentColor">
          <path d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z" stroke="currentColor" stroke-width="2" fill="none" stroke-linecap="round" />
        </svg>
        <svg v-else viewBox="0 0 24 24" width="20" height="20" fill="currentColor">
          <path d="M21 12.79A9 9 0 1111.21 3 7 7 0 0021 12.79z" stroke="currentColor" stroke-width="2" fill="none" stroke-linecap="round" stroke-linejoin="round" />
        </svg>
      </button>

      <template v-if="authStore.isAuthenticated">
        <router-link to="/dashboard" class="nav-link">Dashboard</router-link>
        <router-link to="/stories/new" class="nav-link btn-primary">New Story</router-link>

        <div class="user-menu">
          <img
            v-if="authStore.user?.avatarUrl"
            :src="authStore.user.avatarUrl"
            :alt="authStore.user.displayName"
            class="avatar"
          />
          <span class="user-name">{{ authStore.user?.displayName }}</span>
          <button @click="handleLogout" class="btn-logout">Logout</button>
        </div>
      </template>

      <template v-else>
        <router-link to="/login" class="nav-link btn-primary">Login</router-link>
      </template>
    </div>
  </nav>
</template>

<style scoped>
.navbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 2rem;
  background: var(--color-bg-secondary);
  border-bottom: 1px solid var(--color-border);
}

.brand-link {
  font-size: 1.5rem;
  font-weight: bold;
  color: var(--color-accent);
  text-decoration: none;
}

.navbar-menu {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.nav-link {
  color: var(--color-text-primary);
  text-decoration: none;
  padding: 0.5rem 1rem;
  border-radius: 4px;
  transition: background 0.2s;
}

.nav-link:hover {
  background: var(--color-bg-hover);
}

.btn-primary {
  background: var(--color-accent);
  color: white;
}

.btn-primary:hover {
  background: var(--color-accent-hover);
}

.theme-toggle {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  padding: 0;
  background: transparent;
  border: 1px solid var(--color-border);
  border-radius: 8px;
  color: var(--color-text-primary);
  cursor: pointer;
  transition: background 0.2s, border-color 0.2s;
}

.theme-toggle:hover {
  background: var(--color-bg-hover);
  border-color: var(--color-accent);
}

.user-menu {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
}

.user-name {
  color: var(--color-text-primary);
}

.btn-logout {
  background: transparent;
  border: 1px solid var(--color-border);
  color: var(--color-text-primary);
  padding: 0.5rem 1rem;
  border-radius: 4px;
  cursor: pointer;
}

.btn-logout:hover {
  background: var(--color-bg-hover);
}
</style>
