import { defineStore } from 'pinia';
import { ref } from 'vue';
import type { Story, StoryListItem, CreateStoryRequest, UpdateStoryRequest, StoryType, Visibility } from '../types';
import { storyService } from '../services/stories';

export const useStoryStore = defineStore('story', () => {
  const stories = ref<StoryListItem[]>([]);
  const myStories = ref<StoryListItem[]>([]);
  const currentStory = ref<Story | null>(null);
  const loading = ref(false);
  const error = ref<string | null>(null);
  const pagination = ref({
    page: 1,
    pageSize: 20,
    totalCount: 0,
    totalPages: 0
  });

  async function fetchPublicStories(
    page = 1,
    pageSize = 20,
    storyType?: StoryType,
    tag?: string,
    sortBy = 'newest'
  ) {
    loading.value = true;
    error.value = null;

    try {
      const response = await storyService.getPublicStories(page, pageSize, storyType, tag, sortBy);
      stories.value = response.items;
      pagination.value = {
        page: response.page,
        pageSize: response.pageSize,
        totalCount: response.totalCount,
        totalPages: response.totalPages
      };
    } catch (err: any) {
      error.value = err.response?.data?.error || 'Failed to fetch stories';
    } finally {
      loading.value = false;
    }
  }

  async function fetchMyStories(page = 1, pageSize = 20) {
    loading.value = true;
    error.value = null;

    try {
      const response = await storyService.getMyStories(page, pageSize);
      myStories.value = response.items;
    } catch (err: any) {
      error.value = err.response?.data?.error || 'Failed to fetch stories';
    } finally {
      loading.value = false;
    }
  }

  async function fetchStory(id: string) {
    loading.value = true;
    error.value = null;

    try {
      currentStory.value = await storyService.getById(id);
    } catch (err: any) {
      error.value = err.response?.data?.error || 'Failed to fetch story';
    } finally {
      loading.value = false;
    }
  }

  async function fetchStoryByShareToken(shareToken: string) {
    loading.value = true;
    error.value = null;

    try {
      currentStory.value = await storyService.getByShareToken(shareToken);
    } catch (err: any) {
      error.value = err.response?.data?.error || 'Failed to fetch story';
    } finally {
      loading.value = false;
    }
  }

  async function createStory(request: CreateStoryRequest): Promise<Story | null> {
    loading.value = true;
    error.value = null;

    try {
      const story = await storyService.create(request);
      currentStory.value = story;
      return story;
    } catch (err: any) {
      error.value = err.response?.data?.error || 'Failed to create story';
      return null;
    } finally {
      loading.value = false;
    }
  }

  async function updateStory(id: string, request: UpdateStoryRequest): Promise<Story | null> {
    loading.value = true;
    error.value = null;

    try {
      const story = await storyService.update(id, request);
      currentStory.value = story;
      return story;
    } catch (err: any) {
      error.value = err.response?.data?.error || 'Failed to update story';
      return null;
    } finally {
      loading.value = false;
    }
  }

  async function publishStory(id: string, visibility: Visibility): Promise<Story | null> {
    loading.value = true;
    error.value = null;

    try {
      const story = await storyService.publish(id, visibility);
      currentStory.value = story;
      return story;
    } catch (err: any) {
      error.value = err.response?.data?.error || 'Failed to publish story';
      return null;
    } finally {
      loading.value = false;
    }
  }

  async function deleteStory(id: string): Promise<boolean> {
    loading.value = true;
    error.value = null;

    try {
      await storyService.delete(id);
      myStories.value = myStories.value.filter(s => s.id !== id);
      return true;
    } catch (err: any) {
      error.value = err.response?.data?.error || 'Failed to delete story';
      return false;
    } finally {
      loading.value = false;
    }
  }

  return {
    stories,
    myStories,
    currentStory,
    loading,
    error,
    pagination,
    fetchPublicStories,
    fetchMyStories,
    fetchStory,
    fetchStoryByShareToken,
    createStory,
    updateStory,
    publishStory,
    deleteStory
  };
});
