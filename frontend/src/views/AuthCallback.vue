<script setup lang="ts">
import { onMounted } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { useAuthStore } from '../stores/auth';

const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();

onMounted(async () => {
  const token = route.query.token as string;

  if (token) {
    authStore.setToken(token);
    await authStore.fetchCurrentUser();

    const redirect = route.query.redirect as string;
    router.push(redirect || '/dashboard');
  } else {
    router.push('/login');
  }
});
</script>

<template>
  <div class="callback-page">
    <div class="loading">
      <div class="spinner"></div>
      <p>Signing you in...</p>
    </div>
  </div>
</template>

<style scoped>
.callback-page {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 60vh;
}

.loading {
  text-align: center;
}

.spinner {
  width: 48px;
  height: 48px;
  border: 4px solid #333;
  border-top-color: #646cff;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 1rem;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}
</style>
