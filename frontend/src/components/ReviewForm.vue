<script setup lang="ts">
import { ref } from 'vue';
import { useReviewStore } from '../stores/review';
import { useStoryStore } from '../stores/story';
import { REVIEW_CATEGORIES, type CreateReviewRequest } from '../types';

const props = defineProps<{
  storyId: string;
}>();

const reviewStore = useReviewStore();
const storyStore = useStoryStore();

const isOpen = ref(false);
const form = ref<CreateReviewRequest>({
  clarity: 3,
  ownership: 3,
  impact: 3,
  decisionMaking: 3,
  communication: 3,
  reflection: 3,
  technicalDepth: undefined,
  feedback: '',
  strengths: [],
  improvements: []
});

const includeTechDepth = ref(false);
const strengthInput = ref('');
const improvementInput = ref('');

function addStrength() {
  if (strengthInput.value) {
    form.value.strengths = [...(form.value.strengths || []), strengthInput.value.trim()];
    strengthInput.value = '';
  }
}

function removeStrength(s: string) {
  form.value.strengths = form.value.strengths?.filter(str => str !== s) || [];
}

function addImprovement() {
  if (improvementInput.value) {
    form.value.improvements = [...(form.value.improvements || []), improvementInput.value.trim()];
    improvementInput.value = '';
  }
}

function removeImprovement(i: string) {
  form.value.improvements = form.value.improvements?.filter(imp => imp !== i) || [];
}

async function handleSubmit() {
  const request: CreateReviewRequest = {
    ...form.value,
    technicalDepth: includeTechDepth.value ? form.value.technicalDepth : undefined
  };

  const review = await reviewStore.createReview(props.storyId, request);
  if (review) {
    isOpen.value = false;
    // Refresh story to get updated stats
    await storyStore.fetchStory(props.storyId);
  }
}
</script>

<template>
  <div class="review-form-container">
    <button v-if="!isOpen" @click="isOpen = true" class="btn btn-primary">
      Write a Review
    </button>

    <form v-else @submit.prevent="handleSubmit" class="review-form">
      <h3>Write Your Review</h3>

      <div class="scores-section">
        <div
          v-for="category in REVIEW_CATEGORIES.filter(c => !c.optional)"
          :key="category.key"
          class="score-input"
        >
          <label>
            {{ category.label }}
            <span class="weight">({{ category.weight }}x)</span>
          </label>
          <div class="score-slider">
            <input
              type="range"
              min="1"
              max="5"
              v-model.number="form[category.key as keyof CreateReviewRequest]"
            />
            <span class="score-value">{{ form[category.key as keyof CreateReviewRequest] }}</span>
          </div>
        </div>

        <div class="score-input optional">
          <label>
            <input type="checkbox" v-model="includeTechDepth" />
            Technical Depth
            <span class="weight">(0.6x, optional)</span>
          </label>
          <div v-if="includeTechDepth" class="score-slider">
            <input
              type="range"
              min="1"
              max="5"
              v-model.number="form.technicalDepth"
            />
            <span class="score-value">{{ form.technicalDepth }}</span>
          </div>
        </div>
      </div>

      <div class="form-group">
        <label>Overall Feedback</label>
        <textarea
          v-model="form.feedback"
          placeholder="Share your thoughts on this story..."
          rows="4"
        ></textarea>
      </div>

      <div class="form-group">
        <label>Strengths</label>
        <div class="list-input">
          <div class="items">
            <span v-for="s in form.strengths" :key="s" class="item strength">
              {{ s }}
              <button type="button" @click="removeStrength(s)">&times;</button>
            </span>
          </div>
          <input
            v-model="strengthInput"
            placeholder="Add a strength and press Enter"
            @keydown.enter.prevent="addStrength"
          />
        </div>
      </div>

      <div class="form-group">
        <label>Areas for Improvement</label>
        <div class="list-input">
          <div class="items">
            <span v-for="i in form.improvements" :key="i" class="item improvement">
              {{ i }}
              <button type="button" @click="removeImprovement(i)">&times;</button>
            </span>
          </div>
          <input
            v-model="improvementInput"
            placeholder="Add an improvement and press Enter"
            @keydown.enter.prevent="addImprovement"
          />
        </div>
      </div>

      <div v-if="reviewStore.error" class="error">
        {{ reviewStore.error }}
      </div>

      <div class="form-actions">
        <button type="button" @click="isOpen = false" class="btn btn-secondary">
          Cancel
        </button>
        <button type="submit" class="btn btn-primary" :disabled="reviewStore.loading">
          Submit Review
        </button>
      </div>
    </form>
  </div>
</template>

<style scoped>
.review-form-container {
  margin-bottom: 2rem;
}

.btn {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
}

.btn-primary {
  background: #646cff;
  color: white;
}

.btn-secondary {
  background: #333;
  color: white;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.review-form {
  background: #1a1a2e;
  border: 1px solid #333;
  border-radius: 8px;
  padding: 1.5rem;
}

.review-form h3 {
  margin-bottom: 1.5rem;
}

.scores-section {
  display: grid;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.score-input label {
  display: block;
  margin-bottom: 0.5rem;
  color: #888;
}

.weight {
  font-size: 0.75rem;
  color: #666;
}

.score-slider {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.score-slider input[type="range"] {
  flex: 1;
  accent-color: #646cff;
}

.score-value {
  font-weight: bold;
  width: 1.5rem;
  text-align: center;
}

.optional label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.form-group {
  margin-bottom: 1rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  color: #888;
}

.form-group textarea,
.form-group input[type="text"],
.list-input input {
  width: 100%;
  padding: 0.75rem;
  background: #16162a;
  border: 1px solid #333;
  border-radius: 4px;
  color: white;
  font-family: inherit;
}

.list-input {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.items {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.item {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.875rem;
}

.item.strength {
  background: rgba(66, 184, 131, 0.2);
  color: #42b883;
}

.item.improvement {
  background: rgba(240, 173, 78, 0.2);
  color: #f0ad4e;
}

.item button {
  background: none;
  border: none;
  color: inherit;
  cursor: pointer;
  padding: 0;
  font-size: 1rem;
}

.error {
  color: #ff6b6b;
  padding: 1rem;
  background: rgba(255, 107, 107, 0.1);
  border-radius: 4px;
  margin-bottom: 1rem;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
}
</style>
