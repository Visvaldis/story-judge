<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useStoryStore } from '../stores/story';
import { STORY_TYPES, type StoryType } from '../types';
import StoryCard from '../components/StoryCard.vue';

const storyStore = useStoryStore();

const selectedType = ref<StoryType | undefined>();
const sortBy = ref('newest');

async function loadStories() {
  await storyStore.fetchPublicStories(1, 20, selectedType.value, undefined, sortBy.value);
}

function handleTypeChange(type: StoryType | undefined) {
  selectedType.value = type;
  loadStories();
}

function handleSortChange(sort: string) {
  sortBy.value = sort;
  loadStories();
}

onMounted(loadStories);
</script>

<template>
  <div class="explore">
    <header class="explore-header">
      <h1>Explore Stories</h1>
      <p>Learn from how others structure their behavioral interview stories</p>
    </header>

    <div class="filters">
      <div class="filter-group">
        <label>Story Type:</label>
        <select @change="handleTypeChange(($event.target as HTMLSelectElement).value as StoryType || undefined)">
          <option value="">All Types</option>
          <option v-for="type in STORY_TYPES" :key="type" :value="type">
            {{ type }}
          </option>
        </select>
      </div>

      <div class="filter-group">
        <label>Sort By:</label>
        <select @change="handleSortChange(($event.target as HTMLSelectElement).value)">
          <option value="newest">Newest</option>
          <option value="highest-rated">Highest Rated</option>
          <option value="most-reviewed">Most Reviewed</option>
        </select>
      </div>
    </div>

    <div v-if="storyStore.loading" class="loading">
      Loading...
    </div>

    <div v-else-if="storyStore.stories.length === 0" class="empty-state">
      <p>No public stories found. Be the first to publish!</p>
    </div>

    <div v-else class="stories-grid">
      <StoryCard
        v-for="story in storyStore.stories"
        :key="story.id"
        :story="story"
      />
    </div>
  </div>
</template>

<style scoped>
.explore-header {
  text-align: center;
  margin-bottom: 2rem;
}

.explore-header p {
  color: #888;
}

.filters {
  display: flex;
  gap: 1.5rem;
  margin-bottom: 2rem;
  flex-wrap: wrap;
}

.filter-group {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.filter-group label {
  color: #888;
}

.filter-group select {
  padding: 0.5rem 1rem;
  background: #1a1a2e;
  border: 1px solid #333;
  border-radius: 4px;
  color: white;
}

.stories-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
}

.empty-state {
  text-align: center;
  padding: 3rem;
  color: #888;
}

.loading {
  text-align: center;
  padding: 2rem;
  color: #888;
}
</style>
