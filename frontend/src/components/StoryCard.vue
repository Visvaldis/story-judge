<script setup lang="ts">
import { useRouter } from 'vue-router';
import { useStoryStore } from '../stores/story';
import type { StoryListItem } from '../types';

const props = defineProps<{
  story: StoryListItem;
  showActions?: boolean;
}>();

const router = useRouter();
const storyStore = useStoryStore();

function viewStory() {
  router.push(`/stories/${props.story.id}`);
}

async function deleteStory() {
  if (confirm('Are you sure you want to delete this story?')) {
    await storyStore.deleteStory(props.story.id);
  }
}
</script>

<template>
  <article class="story-card" @click="viewStory">
    <header class="card-header">
      <span class="story-type">{{ story.storyType }}</span>
      <span class="story-status" :class="story.status.toLowerCase()">
        {{ story.status }}
      </span>
    </header>

    <h3 class="card-title">{{ story.title }}</h3>

    <div class="card-meta">
      <img
        v-if="story.authorAvatarUrl"
        :src="story.authorAvatarUrl"
        :alt="story.authorName"
        class="avatar"
      />
      <span class="author">{{ story.authorName }}</span>
    </div>

    <div v-if="story.tags.length > 0" class="tags">
      <span v-for="tag in story.tags.slice(0, 3)" :key="tag" class="tag">
        {{ tag }}
      </span>
      <span v-if="story.tags.length > 3" class="tag more">
        +{{ story.tags.length - 3 }}
      </span>
    </div>

    <footer class="card-footer">
      <div class="stats">
        <span v-if="story.reviewStats.totalReviews > 0" class="stat">
          {{ story.reviewStats.overallScore.toFixed(1) }} / 5
        </span>
        <span class="stat">
          {{ story.reviewStats.totalReviews }} reviews
        </span>
      </div>

      <div v-if="showActions" class="actions" @click.stop>
        <router-link :to="`/stories/${story.id}/edit`" class="action-btn">
          Edit
        </router-link>
        <button @click="deleteStory" class="action-btn delete">
          Delete
        </button>
      </div>
    </footer>
  </article>
</template>

<style scoped>
.story-card {
  background: #1a1a2e;
  border: 1px solid #333;
  border-radius: 8px;
  padding: 1.25rem;
  cursor: pointer;
  transition: transform 0.2s, box-shadow 0.2s;
}

.story-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
}

.card-header {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 0.75rem;
}

.story-type {
  background: #646cff;
  padding: 0.2rem 0.5rem;
  border-radius: 4px;
  font-size: 0.75rem;
}

.story-status {
  padding: 0.2rem 0.5rem;
  border-radius: 4px;
  font-size: 0.75rem;
}

.story-status.draft {
  background: #666;
}

.story-status.published {
  background: #42b883;
}

.card-title {
  margin-bottom: 0.75rem;
  font-size: 1.1rem;
}

.card-meta {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.75rem;
  color: #888;
  font-size: 0.875rem;
}

.avatar {
  width: 24px;
  height: 24px;
  border-radius: 50%;
}

.tags {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-bottom: 0.75rem;
}

.tag {
  background: #333;
  padding: 0.2rem 0.5rem;
  border-radius: 4px;
  font-size: 0.75rem;
}

.card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-top: 1px solid #333;
  padding-top: 0.75rem;
  margin-top: 0.5rem;
}

.stats {
  display: flex;
  gap: 1rem;
  color: #888;
  font-size: 0.875rem;
}

.actions {
  display: flex;
  gap: 0.5rem;
}

.action-btn {
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.75rem;
  text-decoration: none;
  background: #333;
  color: white;
  border: none;
  cursor: pointer;
}

.action-btn.delete {
  background: #dc3545;
}
</style>
