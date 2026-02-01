<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useRouter } from 'vue-router';
import { useStoryStore } from '../stores/story';
import type { StoryListItem } from '../types';

const props = defineProps<{
  data: {
    story: StoryListItem;
    expanded: boolean;
  };
}>();

const emit = defineEmits<{
  (e: 'expand'): void;
  (e: 'collapse'): void;
  (e: 'delete', storyId: string): void;
}>();

const router = useRouter();
const storyStore = useStoryStore();

const editableTitle = ref(props.data.story.title);
const isSaving = ref(false);

// Sync title when story changes
watch(() => props.data.story.title, (newTitle) => {
  editableTitle.value = newTitle;
});

const truncatedTitle = computed(() => {
  const title = props.data.story.title;
  return title.length > 35 ? title.substring(0, 35) + '...' : title;
});

const scoreDisplay = computed(() => {
  const stats = props.data.story.reviewStats;
  if (stats.totalReviews > 0) {
    return `${stats.overallScore.toFixed(1)}/5`;
  }
  return null;
});

function handleNodeClick(e: Event) {
  // Don't toggle if clicking on inputs or buttons
  const target = e.target as Element;
  if (target.tagName === 'INPUT' || target.tagName === 'BUTTON' || target.closest('button') || target.closest('a')) {
    return;
  }

  if (props.data.expanded) {
    emit('collapse');
  } else {
    emit('expand');
  }
}

function handleClose(e: Event) {
  e.stopPropagation();
  emit('collapse');
}

async function saveTitle() {
  if (editableTitle.value === props.data.story.title) return;
  if (!editableTitle.value.trim()) {
    editableTitle.value = props.data.story.title;
    return;
  }

  isSaving.value = true;
  try {
    await storyStore.updateStory(props.data.story.id, { title: editableTitle.value });
  } catch {
    editableTitle.value = props.data.story.title;
  } finally {
    isSaving.value = false;
  }
}

function openFullEditor() {
  router.push(`/stories/${props.data.story.id}/edit`);
}

function handleDelete() {
  if (confirm('Are you sure you want to delete this story?')) {
    emit('delete', props.data.story.id);
  }
}
</script>

<template>
  <div
    class="story-node"
    :class="{ expanded: data.expanded }"
    @click="handleNodeClick"
  >
    <!-- Collapsed State -->
    <template v-if="!data.expanded">
      <header class="node-header">
        <span class="story-type">{{ data.story.storyType }}</span>
        <span class="story-status" :class="data.story.status.toLowerCase()">
          {{ data.story.status }}
        </span>
      </header>
      <h4 class="node-title">{{ truncatedTitle }}</h4>
      <footer class="node-footer">
        <span v-if="scoreDisplay" class="score">{{ scoreDisplay }}</span>
        <span class="reviews">{{ data.story.reviewStats.totalReviews }} reviews</span>
      </footer>
    </template>

    <!-- Expanded State -->
    <template v-else>
      <header class="expanded-header">
        <div class="badges">
          <span class="story-type">{{ data.story.storyType }}</span>
          <span class="story-status" :class="data.story.status.toLowerCase()">
            {{ data.story.status }}
          </span>
        </div>
        <button class="close-btn" @click="handleClose" title="Collapse">
          &times;
        </button>
      </header>

      <div class="expanded-content">
        <input
          v-model="editableTitle"
          class="title-input"
          @blur="saveTitle"
          @keydown.enter="($event.target as HTMLInputElement).blur()"
          :disabled="isSaving"
          placeholder="Story title"
        />

        <div v-if="data.story.tags.length > 0" class="tags">
          <span v-for="tag in data.story.tags.slice(0, 4)" :key="tag" class="tag">
            {{ tag }}
          </span>
          <span v-if="data.story.tags.length > 4" class="tag more">
            +{{ data.story.tags.length - 4 }}
          </span>
        </div>

        <div class="stats">
          <span v-if="scoreDisplay" class="stat">{{ scoreDisplay }}</span>
          <span class="stat">{{ data.story.reviewStats.totalReviews }} reviews</span>
        </div>
      </div>

      <footer class="expanded-footer">
        <button class="action-btn edit" @click="openFullEditor">
          Edit Full
        </button>
        <button class="action-btn delete" @click="handleDelete">
          Delete
        </button>
      </footer>
    </template>
  </div>
</template>

<style scoped>
.story-node {
  background: var(--color-bg-secondary);
  border: 1px solid var(--color-border);
  border-radius: 8px;
  padding: 12px;
  cursor: pointer;
  transition: box-shadow 0.2s, width 0.2s, height 0.2s;
  width: 220px;
  min-height: 100px;
}

.story-node:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
}

.story-node.expanded {
  width: 320px;
  min-height: auto;
  cursor: default;
}

.node-header {
  display: flex;
  gap: 6px;
  margin-bottom: 8px;
}

.story-type {
  background: var(--color-accent);
  color: white;
  padding: 2px 6px;
  border-radius: 4px;
  font-size: 0.7rem;
  font-weight: 500;
}

.story-status {
  padding: 2px 6px;
  border-radius: 4px;
  font-size: 0.7rem;
}

.story-status.draft {
  background: var(--color-text-muted);
  color: white;
}

.story-status.published {
  background: var(--color-success);
  color: white;
}

.node-title {
  font-size: 0.9rem;
  margin: 0 0 8px 0;
  line-height: 1.3;
  color: var(--color-text-primary);
}

.node-footer {
  display: flex;
  gap: 8px;
  font-size: 0.75rem;
  color: var(--color-text-secondary);
}

.score {
  font-weight: 600;
  color: var(--color-accent);
}

/* Expanded State Styles */
.expanded-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 12px;
}

.badges {
  display: flex;
  gap: 6px;
}

.close-btn {
  background: none;
  border: none;
  font-size: 1.4rem;
  cursor: pointer;
  color: var(--color-text-secondary);
  padding: 0;
  line-height: 1;
  width: 24px;
  height: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 4px;
}

.close-btn:hover {
  background: var(--color-bg-hover);
  color: var(--color-text-primary);
}

.expanded-content {
  margin-bottom: 12px;
}

.title-input {
  width: 100%;
  padding: 8px;
  border: 1px solid var(--color-border);
  border-radius: 4px;
  font-size: 1rem;
  font-weight: 600;
  background: var(--color-bg-primary);
  color: var(--color-text-primary);
  margin-bottom: 10px;
}

.title-input:focus {
  outline: none;
  border-color: var(--color-accent);
}

.title-input:disabled {
  opacity: 0.7;
}

.tags {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
  margin-bottom: 10px;
}

.tag {
  background: var(--color-bg-hover);
  padding: 2px 6px;
  border-radius: 4px;
  font-size: 0.7rem;
}

.tag.more {
  background: var(--color-bg-primary);
  color: var(--color-text-secondary);
}

.stats {
  display: flex;
  gap: 12px;
  font-size: 0.8rem;
  color: var(--color-text-secondary);
}

.stat:first-child {
  font-weight: 600;
  color: var(--color-accent);
}

.expanded-footer {
  display: flex;
  gap: 8px;
  padding-top: 12px;
  border-top: 1px solid var(--color-border);
}

.action-btn {
  flex: 1;
  padding: 6px 12px;
  border-radius: 4px;
  font-size: 0.8rem;
  font-weight: 500;
  cursor: pointer;
  border: none;
  transition: opacity 0.2s;
}

.action-btn:hover {
  opacity: 0.9;
}

.action-btn.edit {
  background: var(--color-accent);
  color: white;
}

.action-btn.delete {
  background: var(--color-error);
  color: white;
}
</style>
