<script setup lang="ts">
import type { Review } from '../types';
import { REVIEW_CATEGORIES } from '../types';

defineProps<{
  reviews: Review[];
}>();
</script>

<template>
  <div class="review-list">
    <div v-if="reviews.length === 0" class="empty">
      No reviews yet. Be the first to leave feedback!
    </div>

    <article v-for="review in reviews" :key="review.id" class="review-card">
      <header class="review-header">
        <div class="reviewer-info">
          <img
            v-if="review.reviewerAvatarUrl"
            :src="review.reviewerAvatarUrl"
            :alt="review.reviewerName"
            class="avatar"
          />
          <span class="name">{{ review.reviewerName }}</span>
          <span class="date">{{ new Date(review.createdAt).toLocaleDateString() }}</span>
        </div>
        <div class="overall-score">
          {{ review.overallScore.toFixed(1) }}
        </div>
      </header>

      <div class="scores-grid">
        <div
          v-for="category in REVIEW_CATEGORIES"
          :key="category.key"
          class="score-item"
        >
          <span class="score-label">{{ category.label }}</span>
          <span class="score-value">
            {{
              category.key === 'technicalDepth'
                ? (review.scores.technicalDepth ?? '-')
                : review.scores[category.key as keyof typeof review.scores]
            }}
          </span>
        </div>
      </div>

      <div v-if="review.feedback" class="feedback">
        <h4>Feedback</h4>
        <p>{{ review.feedback }}</p>
      </div>

      <div v-if="review.strengths.length > 0" class="strengths">
        <h4>Strengths</h4>
        <ul>
          <li v-for="strength in review.strengths" :key="strength">{{ strength }}</li>
        </ul>
      </div>

      <div v-if="review.improvements.length > 0" class="improvements">
        <h4>Areas for Improvement</h4>
        <ul>
          <li v-for="improvement in review.improvements" :key="improvement">{{ improvement }}</li>
        </ul>
      </div>
    </article>
  </div>
</template>

<style scoped>
.review-list {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.empty {
  text-align: center;
  padding: 2rem;
  color: var(--color-text-secondary);
  background: var(--color-bg-secondary);
  border-radius: 8px;
}

.review-card {
  background: var(--color-bg-secondary);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  padding: 1.5rem;
}

.review-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.reviewer-info {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
}

.name {
  font-weight: 500;
}

.date {
  color: var(--color-text-muted);
  font-size: 0.875rem;
}

.overall-score {
  background: var(--color-success);
  color: white;
  font-size: 1.25rem;
  font-weight: bold;
  padding: 0.5rem 0.75rem;
  border-radius: 8px;
}

.scores-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
  gap: 0.75rem;
  margin-bottom: 1rem;
  padding: 1rem;
  background: var(--color-bg-tertiary);
  border-radius: 4px;
}

.score-item {
  display: flex;
  justify-content: space-between;
  font-size: 0.875rem;
}

.score-label {
  color: var(--color-text-secondary);
}

.score-value {
  font-weight: 500;
}

.feedback,
.strengths,
.improvements {
  margin-top: 1rem;
}

.feedback h4,
.strengths h4,
.improvements h4 {
  color: var(--color-text-secondary);
  font-size: 0.875rem;
  margin-bottom: 0.5rem;
}

.feedback p {
  line-height: 1.6;
}

.strengths ul,
.improvements ul {
  margin: 0;
  padding-left: 1.25rem;
}

.strengths li,
.improvements li {
  margin-bottom: 0.25rem;
}

.strengths {
  color: var(--color-success);
}

.improvements {
  color: var(--color-warning);
}
</style>
