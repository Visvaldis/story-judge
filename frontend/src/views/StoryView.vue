<script setup lang="ts">
import { onMounted, computed } from 'vue';
import { useRoute } from 'vue-router';
import { useStoryStore } from '../stores/story';
import { useReviewStore } from '../stores/review';
import { useAuthStore } from '../stores/auth';
import ReviewList from '../components/ReviewList.vue';
import ReviewForm from '../components/ReviewForm.vue';
import CoverageChecklist from '../components/CoverageChecklist.vue';

const route = useRoute();
const storyStore = useStoryStore();
const reviewStore = useReviewStore();
const authStore = useAuthStore();

const story = computed(() => storyStore.currentStory);
const isAuthor = computed(() => story.value?.authorId === authStore.user?.id);
const canReview = computed(() =>
  authStore.isAuthenticated &&
  !isAuthor.value &&
  story.value?.status === 'Published'
);

onMounted(async () => {
  await storyStore.fetchStory(route.params.id as string);
  if (story.value) {
    await reviewStore.fetchReviewsForStory(story.value.id);
  }
});

function getShareUrl() {
  if (!story.value?.shareToken) return '';
  return `${window.location.origin}/shared/${story.value.shareToken}`;
}

function copyShareLink() {
  navigator.clipboard.writeText(getShareUrl());
}
</script>

<template>
  <div v-if="storyStore.loading" class="loading">
    Loading...
  </div>

  <div v-else-if="!story" class="error">
    Story not found
  </div>

  <div v-else class="story-view">
    <header class="story-header">
      <div class="story-meta">
        <span class="story-type">{{ story.storyType }}</span>
        <span class="story-status" :class="story.status.toLowerCase()">
          {{ story.status }}
        </span>
        <span v-if="story.visibility === 'Unlisted'" class="visibility">
          Unlisted
        </span>
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
        <span class="date">{{ new Date(story.createdAt).toLocaleDateString() }}</span>
      </div>

      <div v-if="story.tags.length > 0" class="tags">
        <span v-for="tag in story.tags" :key="tag" class="tag">{{ tag }}</span>
      </div>

      <div v-if="isAuthor" class="author-actions">
        <router-link :to="`/stories/${story.id}/edit`" class="btn btn-secondary">
          Edit
        </router-link>
        <button
          v-if="story.visibility === 'Unlisted' && story.shareToken"
          @click="copyShareLink"
          class="btn btn-secondary"
        >
          Copy Share Link
        </button>
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

        <section class="reviews-section">
          <h2>Reviews ({{ story.reviewStats.totalReviews }})</h2>

          <div v-if="story.reviewStats.totalReviews > 0" class="review-stats">
            <div class="overall-score">
              <span class="score">{{ story.reviewStats.overallScore.toFixed(1) }}</span>
              <span class="label">Overall Score</span>
            </div>
          </div>

          <ReviewForm v-if="canReview" :story-id="story.id" />

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
  color: #888;
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
  background: #646cff;
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.875rem;
}

.story-status {
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.875rem;
}

.story-status.draft {
  background: #666;
}

.story-status.published {
  background: #42b883;
}

.visibility {
  background: #f0ad4e;
  color: #333;
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.875rem;
}

.author-info {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: #888;
  margin-bottom: 1rem;
}

.avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
}

.date {
  color: #666;
}

.tags {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 1rem;
}

.tag {
  background: #333;
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.875rem;
}

.author-actions {
  display: flex;
  gap: 0.5rem;
}

.btn {
  padding: 0.5rem 1rem;
  border-radius: 4px;
  text-decoration: none;
  font-weight: 500;
  cursor: pointer;
  border: none;
}

.btn-secondary {
  background: #333;
  color: white;
}

.story-content {
  display: grid;
  grid-template-columns: 1fr 300px;
  gap: 2rem;
}

.star-section {
  margin-bottom: 2rem;
  padding: 1.5rem;
  background: #1a1a2e;
  border-radius: 8px;
  border: 1px solid #333;
}

.star-section h2 {
  color: #646cff;
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

.review-stats {
  margin-bottom: 2rem;
}

.overall-score {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 1.5rem;
  background: #1a1a2e;
  border-radius: 8px;
  width: fit-content;
}

.overall-score .score {
  font-size: 2.5rem;
  font-weight: bold;
  color: #42b883;
}

.overall-score .label {
  color: #888;
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
