<script setup lang="ts">
import { onMounted } from 'vue';
import { useStoryStore } from '../stores/story';
import { useAuthStore } from '../stores/auth';
import { useWhiteboardStore } from '../stores/whiteboard';
import StoryCard from '../components/StoryCard.vue';
import WhiteboardView from '../components/WhiteboardView.vue';

const storyStore = useStoryStore();
const authStore = useAuthStore();
const whiteboardStore = useWhiteboardStore();

onMounted(async () => {
  await storyStore.fetchMyStories();
});
</script>

<template>
  <div class="dashboard">
    <header class="dashboard-header">
      <h1>Welcome, {{ authStore.user?.displayName }}</h1>
      <div class="header-actions">
        <button
          class="btn btn-secondary view-toggle"
          @click="whiteboardStore.toggleViewMode()"
          :title="whiteboardStore.viewMode === 'grid' ? 'Switch to whiteboard view' : 'Switch to grid view'"
        >
          <svg v-if="whiteboardStore.viewMode === 'grid'" class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <rect x="3" y="3" width="7" height="7" rx="1" />
            <rect x="14" y="3" width="7" height="7" rx="1" />
            <rect x="3" y="14" width="7" height="7" rx="1" />
            <rect x="14" y="14" width="7" height="7" rx="1" />
          </svg>
          <svg v-else class="icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <rect x="3" y="3" width="18" height="18" rx="2" />
            <line x1="3" y1="9" x2="21" y2="9" />
            <line x1="9" y1="21" x2="9" y2="9" />
          </svg>
          {{ whiteboardStore.viewMode === 'grid' ? 'Whiteboard' : 'Grid' }}
        </button>
        <router-link to="/stories/new" class="btn btn-primary">
          + New Story
        </router-link>
      </div>
    </header>

    <section class="my-stories">
      <h2>My Stories</h2>

      <div v-if="storyStore.loading" class="loading">
        Loading...
      </div>

      <!-- Grid View -->
      <template v-else-if="whiteboardStore.viewMode === 'grid'">
        <div v-if="storyStore.myStories.length === 0" class="empty-state">
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
      </template>

      <!-- Whiteboard View -->
      <WhiteboardView v-else />
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

.header-actions {
  display: flex;
  gap: 0.75rem;
  align-items: center;
}

.btn {
  padding: 0.75rem 1.5rem;
  border-radius: 8px;
  text-decoration: none;
  font-weight: 600;
  cursor: pointer;
  border: none;
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
}

.btn-primary {
  background: var(--color-accent);
  color: white;
}

.btn-secondary {
  background: var(--color-bg-secondary);
  color: var(--color-text-primary);
  border: 1px solid var(--color-border);
}

.btn-secondary:hover {
  background: var(--color-bg-hover);
}

.view-toggle .icon {
  width: 18px;
  height: 18px;
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
  background: var(--color-bg-secondary);
  border-radius: 8px;
  border: 1px solid var(--color-border);
}

.empty-state p {
  color: var(--color-text-secondary);
  margin-bottom: 1rem;
}

.loading {
  text-align: center;
  padding: 2rem;
  color: var(--color-text-secondary);
}

/* Hide toggle button on mobile */
@media (max-width: 768px) {
  .view-toggle {
    display: none;
  }
}
</style>
