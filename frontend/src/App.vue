<script setup lang="ts">
import { onMounted } from 'vue';
import { useAuthStore } from './stores/auth';
import { useThemeStore } from './stores/theme';
import Navbar from './components/Navbar.vue';

const authStore = useAuthStore();
useThemeStore(); // Initialize theme on app load

onMounted(async () => {
  await authStore.fetchCurrentUser();
});
</script>

<template>
  <div class="app">
    <Navbar />
    <main class="main-content">
      <router-view />
    </main>
  </div>
</template>

<style scoped>
.app {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.main-content {
  flex: 1;
  padding: 2rem;
  max-width: 1200px;
  margin: 0 auto;
  width: 100%;
}
</style>
