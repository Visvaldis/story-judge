<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { useStoryStore } from '../stores/story';
import { STORY_TYPES, VISIBILITY_OPTIONS, type StoryType, type Visibility, type CreateStoryRequest } from '../types';
import CoverageChecklist from '../components/CoverageChecklist.vue';

const router = useRouter();
const route = useRoute();
const storyStore = useStoryStore();

const isEditing = computed(() => !!route.params.id);

const form = ref<CreateStoryRequest>({
  title: '',
  storyType: 'Behavioral',
  situation: '',
  task: '',
  action: '',
  result: '',
  reflection: '',
  tags: [],
  visibility: 'Private'
});

const tagInput = ref('');

onMounted(async () => {
  if (isEditing.value) {
    await storyStore.fetchStory(route.params.id as string);
    if (storyStore.currentStory) {
      form.value = {
        title: storyStore.currentStory.title,
        storyType: storyStore.currentStory.storyType as StoryType,
        situation: storyStore.currentStory.situation,
        task: storyStore.currentStory.task,
        action: storyStore.currentStory.action,
        result: storyStore.currentStory.result,
        reflection: storyStore.currentStory.reflection || '',
        tags: storyStore.currentStory.tags,
        visibility: storyStore.currentStory.visibility as Visibility
      };
    }
  }
});

function addTag() {
  if (tagInput.value && !form.value.tags?.includes(tagInput.value)) {
    form.value.tags = [...(form.value.tags || []), tagInput.value.trim()];
    tagInput.value = '';
  }
}

function removeTag(tag: string) {
  form.value.tags = form.value.tags?.filter(t => t !== tag) || [];
}

async function handleSubmit() {
  if (isEditing.value) {
    const story = await storyStore.updateStory(route.params.id as string, form.value);
    if (story) {
      router.push(`/stories/${story.id}`);
    }
  } else {
    const story = await storyStore.createStory(form.value);
    if (story) {
      router.push(`/stories/${story.id}`);
    }
  }
}

async function handlePublish() {
  if (!isEditing.value || !storyStore.currentStory) return;

  const story = await storyStore.publishStory(
    storyStore.currentStory.id,
    form.value.visibility
  );
  if (story) {
    router.push(`/stories/${story.id}`);
  }
}
</script>

<template>
  <div class="story-editor">
    <h1>{{ isEditing ? 'Edit Story' : 'Create New Story' }}</h1>

    <form @submit.prevent="handleSubmit" class="editor-form">
      <div class="form-row">
        <div class="form-group flex-1">
          <label for="title">Title</label>
          <input
            id="title"
            v-model="form.title"
            type="text"
            placeholder="Give your story a memorable title"
            required
          />
        </div>

        <div class="form-group">
          <label for="storyType">Story Type</label>
          <select id="storyType" v-model="form.storyType">
            <option v-for="type in STORY_TYPES" :key="type" :value="type">
              {{ type }}
            </option>
          </select>
        </div>

        <div class="form-group">
          <label for="visibility">Visibility</label>
          <select id="visibility" v-model="form.visibility">
            <option v-for="vis in VISIBILITY_OPTIONS" :key="vis" :value="vis">
              {{ vis }}
            </option>
          </select>
        </div>
      </div>

      <div class="star-sections">
        <div class="form-group">
          <label for="situation">Situation</label>
          <textarea
            id="situation"
            v-model="form.situation"
            placeholder="Describe the context and background..."
            rows="4"
          ></textarea>
        </div>

        <div class="form-group">
          <label for="task">Task</label>
          <textarea
            id="task"
            v-model="form.task"
            placeholder="What was your responsibility or goal?"
            rows="4"
          ></textarea>
        </div>

        <div class="form-group">
          <label for="action">Action</label>
          <textarea
            id="action"
            v-model="form.action"
            placeholder="What specific steps did you take?"
            rows="6"
          ></textarea>
        </div>

        <div class="form-group">
          <label for="result">Result</label>
          <textarea
            id="result"
            v-model="form.result"
            placeholder="What was the outcome? Include metrics if possible."
            rows="4"
          ></textarea>
        </div>

        <div class="form-group">
          <label for="reflection">Reflection (Optional)</label>
          <textarea
            id="reflection"
            v-model="form.reflection"
            placeholder="What did you learn from this experience?"
            rows="3"
          ></textarea>
        </div>
      </div>

      <div class="form-group">
        <label>Tags</label>
        <div class="tags-input">
          <div class="tags-list">
            <span v-for="tag in form.tags" :key="tag" class="tag">
              {{ tag }}
              <button type="button" @click="removeTag(tag)">&times;</button>
            </span>
          </div>
          <input
            v-model="tagInput"
            type="text"
            placeholder="Add a tag and press Enter"
            @keydown.enter.prevent="addTag"
          />
        </div>
      </div>

      <div v-if="storyStore.error" class="error">
        {{ storyStore.error }}
      </div>

      <div class="form-actions">
        <button type="submit" class="btn btn-primary" :disabled="storyStore.loading">
          {{ isEditing ? 'Save Changes' : 'Create Story' }}
        </button>

        <button
          v-if="isEditing && storyStore.currentStory?.status === 'Draft'"
          type="button"
          class="btn btn-success"
          @click="handlePublish"
          :disabled="storyStore.loading"
        >
          Publish
        </button>
      </div>
    </form>

    <aside v-if="isEditing && storyStore.currentStory" class="coverage-sidebar">
      <CoverageChecklist :coverage="storyStore.currentStory.coverage" />
    </aside>
  </div>
</template>

<style scoped>
.story-editor {
  display: grid;
  grid-template-columns: 1fr 300px;
  gap: 2rem;
}

.story-editor h1 {
  grid-column: 1 / -1;
}

.editor-form {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.form-row {
  display: flex;
  gap: 1rem;
}

.flex-1 {
  flex: 1;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.form-group label {
  font-weight: 500;
  color: var(--color-text-secondary);
}

.form-group input,
.form-group textarea,
.form-group select {
  padding: 0.75rem;
  background: var(--color-bg-secondary);
  border: 1px solid var(--color-border);
  border-radius: 4px;
  color: var(--color-text-primary);
  font-family: inherit;
}

.form-group textarea {
  resize: vertical;
}

.star-sections {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  padding: 1.5rem;
  background: var(--color-bg-secondary);
  border-radius: 8px;
  border: 1px solid var(--color-border);
}

.tags-input {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.tags-list {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.tag {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  padding: 0.25rem 0.5rem;
  background: var(--color-accent);
  border-radius: 4px;
  font-size: 0.875rem;
}

.tag button {
  background: none;
  border: none;
  color: white;
  cursor: pointer;
  padding: 0;
  font-size: 1rem;
}

.form-actions {
  display: flex;
  gap: 1rem;
}

.btn {
  padding: 0.75rem 1.5rem;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-primary {
  background: var(--color-accent);
  color: white;
}

.btn-success {
  background: var(--color-success);
  color: white;
}

.error {
  color: var(--color-error);
  padding: 1rem;
  background: rgba(239, 68, 68, 0.1);
  border-radius: 4px;
}

.coverage-sidebar {
  position: sticky;
  top: 2rem;
  height: fit-content;
}

@media (max-width: 768px) {
  .story-editor {
    grid-template-columns: 1fr;
  }

  .form-row {
    flex-direction: column;
  }
}
</style>
