<script setup lang="ts">
import { useAuthStore } from '../stores/auth';

const authStore = useAuthStore();
</script>

<template>
  <div class="home">
    <section class="hero">
      <h1>StoryJudge</h1>
      <p class="tagline">Master your behavioral interview stories with peer feedback</p>

      <div class="features">
        <div class="feature">
          <h3>Write STAR Stories</h3>
          <p>Structure your experiences using the proven STAR format for maximum impact</p>
        </div>
        <div class="feature">
          <h3>Get Expert Feedback</h3>
          <p>Receive detailed rubric-based reviews from peers who understand interviews</p>
        </div>
        <div class="feature">
          <h3>Track Your Coverage</h3>
          <p>See which interview signals your stories cover and identify gaps</p>
        </div>
      </div>

      <div class="cta-buttons">
        <router-link v-if="!authStore.isAuthenticated" to="/login" class="btn btn-primary">
          Get Started
        </router-link>
        <router-link v-else to="/dashboard" class="btn btn-primary">
          Go to Dashboard
        </router-link>
        <router-link to="/explore" class="btn btn-secondary">
          Explore Stories
        </router-link>
      </div>
    </section>
  </div>
</template>

<style scoped>
.home {
  text-align: center;
  padding: 2rem 0;
}

.hero h1 {
  font-size: 3rem;
  margin-bottom: 0.5rem;
  background: linear-gradient(135deg, #646cff 0%, #42b883 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.tagline {
  font-size: 1.25rem;
  color: #888;
  margin-bottom: 3rem;
}

.features {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 2rem;
  margin-bottom: 3rem;
}

.feature {
  background: #1a1a2e;
  padding: 2rem;
  border-radius: 8px;
  border: 1px solid #333;
}

.feature h3 {
  color: #646cff;
  margin-bottom: 0.5rem;
}

.feature p {
  color: #888;
}

.cta-buttons {
  display: flex;
  gap: 1rem;
  justify-content: center;
}

.btn {
  padding: 1rem 2rem;
  border-radius: 8px;
  text-decoration: none;
  font-weight: 600;
  transition: transform 0.2s, box-shadow 0.2s;
}

.btn:hover {
  transform: translateY(-2px);
}

.btn-primary {
  background: #646cff;
  color: white;
}

.btn-secondary {
  background: transparent;
  border: 2px solid #646cff;
  color: #646cff;
}
</style>
