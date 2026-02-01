<script setup lang="ts">
import { onMounted, computed } from 'vue';
import { useRoute } from 'vue-router';
import { useStoryStore } from '../stores/story';
import { useReviewStore } from '../stores/review';
import ReviewList from '../components/ReviewList.vue';
import CoverageChecklist from '../components/CoverageChecklist.vue';

const route = useRoute();
const storyStore = useStoryStore();
const reviewStore = useReviewStore();

const story = computed(() => storyStore.currentStory);
const shareToken = computed(() => route.params.shareToken as string);

onMounted(async () => {
  await storyStore.fetchStoryByShareToken(shareToken.value);
  if (story.value) {
    await reviewStore.fetchReviewsForStory(story.value.id, shareToken.value);
  }
});
</script>

<template>
  <div v-if="storyStore.loading" class="loading">
    Loading...
  </div>

  <div v-else-if="!story" class="error">
    Story not found or link expired
  </div>

  <div v-else class="story-view">
    <div class="shared-notice">
      This is a shared link to an unlisted story
    </div>

    <header class="story-header">
      <div class="story-meta">
        <span class="story-type">{{ story.storyType }}</span>
      </div>

      <h1>{{ story.title }}</h1>

      <div class="author-info">
        <img
          v-if="story.authorAvatarUrl"
          :src="story.authorAvatarUrl"
          :alt="story.authorName"
          class="avatar"
        />
        <span>{{ story.authorName }}</span>
      </div>

      <div v-if="story.tags.length > 0" class="tags">
        <span v-for="tag in story.tags" :key="tag" class="tag">{{ tag }}</span>
      </div>
    </header>

    <div class="story-content">
      <main class="story-body">
        <section class="star-section">
          <h2>Situation</h2>
          <p>{{ story.situation }}</p>
        </section>

        <section class="star-section">
          <h2>Task</h2>
          <p>{{ story.task }}</p>
        </section>

        <section class="star-section">
          <h2>Action</h2>
          <p>{{ story.action }}</p>
        </section>

        <section class="star-section">
          <h2>Result</h2>
          <p>{{ story.result }}</p>
        </section>

        <section v-if="story.reflection" class="star-section">
          <h2>Reflection</h2>
          <p>{{ story.reflection }}</p>
        </section>

        <section v-if="reviewStore.reviews.length > 0" class="reviews-section">
          <h2>Reviews ({{ reviewStore.reviews.length }})</h2>
          <ReviewList :reviews="reviewStore.reviews" />
        </section>
      </main>

      <aside class="story-sidebar">
        <CoverageChecklist :coverage="story.coverage" :readonly="true" />
      </aside>
    </div>
  </div>
</template>

<style scoped>
.loading, .error {
  text-align: center;
  padding: 2rem;
  color: var(--color-text-secondary);
}

.shared-notice {
  background: var(--color-warning);
  color: #1a1a2e;
  padding: 0.75rem 1rem;
  border-radius: 8px;
  margin-bottom: 1.5rem;
  text-align: center;
}

.story-header {
  margin-bottom: 2rem;
}

.story-meta {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 0.5rem;
}

.story-type {
  background: var(--color-accent);
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.875rem;
}

.author-info {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: var(--color-text-secondary);
  margin-bottom: 1rem;
}

.avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
}

.tags {
  display: flex;
  gap: 0.5rem;
}

.tag {
  background: var(--color-bg-hover);
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.875rem;
}

.story-content {
  display: grid;
  grid-template-columns: 1fr 300px;
  gap: 2rem;
}

.star-section {
  margin-bottom: 2rem;
  padding: 1.5rem;
  background: var(--color-bg-secondary);
  border-radius: 8px;
  border: 1px solid var(--color-border);
}

.star-section h2 {
  color: var(--color-accent);
  font-size: 1rem;
  text-transform: uppercase;
  margin-bottom: 0.75rem;
}

.star-section p {
  white-space: pre-wrap;
  line-height: 1.6;
}

.reviews-section h2 {
  margin-bottom: 1.5rem;
}

.story-sidebar {
  position: sticky;
  top: 2rem;
  height: fit-content;
}

@media (max-width: 768px) {
  .story-content {
    grid-template-columns: 1fr;
  }
}
</style>
