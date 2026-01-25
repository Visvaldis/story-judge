<script setup lang="ts">
import { onMounted } from 'vue';
import { useStoryStore } from '../stores/story';
import { useAuthStore } from '../stores/auth';
import StoryCard from '../components/StoryCard.vue';

const storyStore = useStoryStore();
const authStore = useAuthStore();

onMounted(async () => {
  await storyStore.fetchMyStories();
});
</script>

<template>
  <div class="dashboard">
    <header class="dashboard-header">
      <h1>Welcome, {{ authStore.user?.displayName }}</h1>
      <router-link to="/stories/new" class="btn btn-primary">
        + New Story
      </router-link>
    </header>

    <section class="my-stories">
      <h2>My Stories</h2>

      <div v-if="storyStore.loading" class="loading">
        Loading...
      </div>

      <div v-else-if="storyStore.myStories.length === 0" class="empty-state">
        <p>You haven't created any stories yet.</p>
        <router-link to="/stories/new" class="btn btn-primary">
          Create Your First Story
        </router-link>
      </div>

      <div v-else class="stories-grid">
        <StoryCard
          v-for="story in storyStore.myStories"
          :key="story.id"
          :story="story"
          :show-actions="true"
        />
      </div>
    </section>
  </div>
</template>

<style scoped>
.dashboard-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.btn {
  padding: 0.75rem 1.5rem;
  border-radius: 8px;
  text-decoration: none;
  font-weight: 600;
}

.btn-primary {
  background: #646cff;
  color: white;
}

.my-stories h2 {
  margin-bottom: 1.5rem;
}

.stories-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
}

.empty-state {
  text-align: center;
  padding: 3rem;
  background: #1a1a2e;
  border-radius: 8px;
  border: 1px solid #333;
}

.empty-state p {
  color: #888;
  margin-bottom: 1rem;
}

.loading {
  text-align: center;
  padding: 2rem;
  color: #888;
}
</style>
