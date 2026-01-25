<script setup lang="ts">
import { useAuthStore } from '../stores/auth';
import { useRouter } from 'vue-router';

const authStore = useAuthStore();
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
  background: #1a1a2e;
  border-bottom: 1px solid #333;
}

.brand-link {
  font-size: 1.5rem;
  font-weight: bold;
  color: #646cff;
  text-decoration: none;
}

.navbar-menu {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.nav-link {
  color: #ffffffde;
  text-decoration: none;
  padding: 0.5rem 1rem;
  border-radius: 4px;
  transition: background 0.2s;
}

.nav-link:hover {
  background: #333;
}

.btn-primary {
  background: #646cff;
  color: white;
}

.btn-primary:hover {
  background: #535bf2;
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
  color: #ffffffde;
}

.btn-logout {
  background: transparent;
  border: 1px solid #666;
  color: #ffffffde;
  padding: 0.5rem 1rem;
  border-radius: 4px;
  cursor: pointer;
}

.btn-logout:hover {
  background: #333;
}
</style>
