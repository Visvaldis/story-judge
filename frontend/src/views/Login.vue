<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/auth';

const router = useRouter();
const authStore = useAuthStore();

const isRegisterMode = ref(false);
const email = ref('');
const password = ref('');
const displayName = ref('');

async function handleSubmit() {
  if (isRegisterMode.value) {
    const success = await authStore.register({
      email: email.value,
      password: password.value,
      displayName: displayName.value
    });
    if (success) {
      router.push('/dashboard');
    }
  } else {
    const success = await authStore.login({
      email: email.value,
      password: password.value
    });
    if (success) {
      router.push('/dashboard');
    }
  }
}

</script>

<template>
  <div class="login-page">
    <div class="login-card">
      <h1>Welcome to StoryJudge</h1>
      <p>Sign in to start crafting and reviewing behavioral interview stories</p>

      <div class="tabs">
        <button :class="{ active: !isRegisterMode }" @click="isRegisterMode = false">
          Sign In
        </button>
        <button :class="{ active: isRegisterMode }" @click="isRegisterMode = true">
          Register
        </button>
      </div>

      <form class="email-form" @submit.prevent="handleSubmit">
        <div v-if="authStore.error" class="error-message">
          {{ authStore.error }}
        </div>

        <div v-if="isRegisterMode" class="form-group">
          <label for="displayName">Display Name</label>
          <input
            id="displayName"
            v-model="displayName"
            type="text"
            placeholder="Your name"
            required
            minlength="2"
          />
        </div>

        <div class="form-group">
          <label for="email">Email</label>
          <input
            id="email"
            v-model="email"
            type="email"
            placeholder="you@example.com"
            required
          />
        </div>

        <div class="form-group">
          <label for="password">Password</label>
          <input
            id="password"
            v-model="password"
            type="password"
            placeholder="********"
            required
            :minlength="isRegisterMode ? 8 : undefined"
          />
          <span v-if="isRegisterMode" class="hint">Minimum 8 characters</span>
        </div>

        <button type="submit" class="submit-btn" :disabled="authStore.loading">
          {{ authStore.loading ? 'Please wait...' : (isRegisterMode ? 'Create Account' : 'Sign In') }}
        </button>
      </form>

      <div class="divider">
        <span>or continue with</span>
      </div>

      <div class="oauth-buttons">
        <a :href="authStore.getGoogleLoginUrl()" class="oauth-btn google">
          <svg viewBox="0 0 24 24" width="20" height="20">
            <path fill="currentColor" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" />
            <path fill="currentColor" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" />
            <path fill="currentColor" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z" />
            <path fill="currentColor" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" />
          </svg>
          Google
        </a>
      </div>

      <p class="note">
        By signing in, you agree to our Terms of Service and Privacy Policy
      </p>
    </div>
  </div>
</template>

<style scoped>
.login-page {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 60vh;
  padding: 2rem;
}

.login-card {
  background: var(--color-bg-secondary);
  padding: 2.5rem;
  border-radius: 12px;
  border: 1px solid var(--color-border);
  text-align: center;
  max-width: 420px;
  width: 100%;
}

.login-card h1 {
  margin-bottom: 0.5rem;
  font-size: 1.75rem;
}

.login-card > p {
  color: var(--color-text-secondary);
  margin-bottom: 1.5rem;
}

.tabs {
  display: flex;
  background: var(--color-bg-tertiary);
  border-radius: 8px;
  padding: 4px;
  margin-bottom: 1.5rem;
}

.tabs button {
  flex: 1;
  padding: 0.75rem;
  border: none;
  background: transparent;
  color: var(--color-text-secondary);
  font-weight: 500;
  cursor: pointer;
  border-radius: 6px;
  transition: all 0.2s;
}

.tabs button.active {
  background: var(--color-accent);
  color: white;
}

.tabs button:hover:not(.active) {
  color: var(--color-text-primary);
}

.email-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  text-align: left;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.form-group label {
  font-size: 0.875rem;
  color: var(--color-text-secondary);
}

.form-group input {
  padding: 0.75rem 1rem;
  border: 1px solid var(--color-border);
  border-radius: 8px;
  background: var(--color-bg-tertiary);
  color: var(--color-text-primary);
  font-size: 1rem;
}

.form-group input:focus {
  outline: none;
  border-color: var(--color-accent);
}

.form-group .hint {
  font-size: 0.75rem;
  color: var(--color-text-muted);
}

.error-message {
  background: rgba(239, 68, 68, 0.1);
  border: 1px solid var(--color-error);
  color: var(--color-error);
  padding: 0.75rem;
  border-radius: 8px;
  font-size: 0.875rem;
}

.submit-btn {
  padding: 0.875rem;
  background: var(--color-accent);
  color: white;
  border: none;
  border-radius: 8px;
  font-size: 1rem;
  font-weight: 500;
  cursor: pointer;
  transition: background 0.2s;
  margin-top: 0.5rem;
}

.submit-btn:hover:not(:disabled) {
  background: var(--color-accent-hover);
}

.submit-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.divider {
  display: flex;
  align-items: center;
  margin: 1.5rem 0;
}

.divider::before,
.divider::after {
  content: '';
  flex: 1;
  height: 1px;
  background: var(--color-border);
}

.divider span {
  padding: 0 1rem;
  color: var(--color-text-muted);
  font-size: 0.875rem;
}

.oauth-buttons {
  display: flex;
  justify-content: center;
}

.oauth-btn {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  padding: 0.75rem;
  border-radius: 8px;
  text-decoration: none;
  font-weight: 500;
  font-size: 0.875rem;
  transition: transform 0.2s, box-shadow 0.2s;
}

.oauth-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
}

.oauth-btn.google {
  background: white;
  color: #333;
}

.note {
  margin-top: 1.5rem;
  font-size: 0.75rem;
  color: var(--color-text-muted);
}
</style>
