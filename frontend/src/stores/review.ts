import { defineStore } from 'pinia';
import { ref } from 'vue';
import type { Review, CreateReviewRequest } from '../types';
import { reviewService } from '../services/reviews';

export const useReviewStore = defineStore('review', () => {
  const reviews = ref<Review[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);

  async function fetchReviewsForStory(storyId: string, shareToken?: string) {
    loading.value = true;
    error.value = null;

    try {
      reviews.value = await reviewService.getByStoryId(storyId, shareToken);
    } catch (err: any) {
      error.value = err.response?.data?.error || 'Failed to fetch reviews';
    } finally {
      loading.value = false;
    }
  }

  async function createReview(storyId: string, request: CreateReviewRequest): Promise<Review | null> {
    loading.value = true;
    error.value = null;

    try {
      const review = await reviewService.create(storyId, request);
      reviews.value.unshift(review);
      return review;
    } catch (err: any) {
      error.value = err.response?.data?.error || 'Failed to create review';
      return null;
    } finally {
      loading.value = false;
    }
  }

  async function updateReview(id: string, request: Partial<CreateReviewRequest>): Promise<Review | null> {
    loading.value = true;
    error.value = null;

    try {
      const review = await reviewService.update(id, request);
      const index = reviews.value.findIndex(r => r.id === id);
      if (index !== -1) {
        reviews.value[index] = review;
      }
      return review;
    } catch (err: any) {
      error.value = err.response?.data?.error || 'Failed to update review';
      return null;
    } finally {
      loading.value = false;
    }
  }

  async function deleteReview(id: string): Promise<boolean> {
    loading.value = true;
    error.value = null;

    try {
      await reviewService.delete(id);
      reviews.value = reviews.value.filter(r => r.id !== id);
      return true;
    } catch (err: any) {
      error.value = err.response?.data?.error || 'Failed to delete review';
      return false;
    } finally {
      loading.value = false;
    }
  }

  return {
    reviews,
    loading,
    error,
    fetchReviewsForStory,
    createReview,
    updateReview,
    deleteReview
  };
});
