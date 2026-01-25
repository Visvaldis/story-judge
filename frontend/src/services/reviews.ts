import api from './api';
import type { Review, CreateReviewRequest } from '../types';

export const reviewService = {
  async getByStoryId(storyId: string, shareToken?: string): Promise<Review[]> {
    const params = shareToken ? `?shareToken=${shareToken}` : '';
    const response = await api.get<Review[]>(`/reviews/story/${storyId}${params}`);
    return response.data;
  },

  async getById(id: string): Promise<Review> {
    const response = await api.get<Review>(`/reviews/${id}`);
    return response.data;
  },

  async create(storyId: string, request: CreateReviewRequest): Promise<Review> {
    const response = await api.post<Review>(`/reviews/story/${storyId}`, request);
    return response.data;
  },

  async update(id: string, request: Partial<CreateReviewRequest>): Promise<Review> {
    const response = await api.put<Review>(`/reviews/${id}`, request);
    return response.data;
  },

  async delete(id: string): Promise<void> {
    await api.delete(`/reviews/${id}`);
  }
};
