import api from './api';
import type {
  Story,
  StoryListItem,
  PaginatedResponse,
  CreateStoryRequest,
  UpdateStoryRequest,
  StoryType,
  Visibility
} from '../types';

export const storyService = {
  async getPublicStories(
    page = 1,
    pageSize = 20,
    storyType?: StoryType,
    tag?: string,
    sortBy = 'newest'
  ): Promise<PaginatedResponse<StoryListItem>> {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
      sortBy
    });
    if (storyType) params.append('storyType', storyType);
    if (tag) params.append('tag', tag);

    const response = await api.get<PaginatedResponse<StoryListItem>>(`/stories?${params}`);
    return response.data;
  },

  async getMyStories(page = 1, pageSize = 20): Promise<PaginatedResponse<StoryListItem>> {
    const response = await api.get<PaginatedResponse<StoryListItem>>(`/stories/my?page=${page}&pageSize=${pageSize}`);
    return response.data;
  },

  async getById(id: string): Promise<Story> {
    const response = await api.get<Story>(`/stories/${id}`);
    return response.data;
  },

  async getByShareToken(shareToken: string): Promise<Story> {
    const response = await api.get<Story>(`/stories/shared/${shareToken}`);
    return response.data;
  },

  async create(request: CreateStoryRequest): Promise<Story> {
    const response = await api.post<Story>('/stories', request);
    return response.data;
  },

  async update(id: string, request: UpdateStoryRequest): Promise<Story> {
    const response = await api.put<Story>(`/stories/${id}`, request);
    return response.data;
  },

  async publish(id: string, visibility: Visibility): Promise<Story> {
    const response = await api.post<Story>(`/stories/${id}/publish`, { visibility });
    return response.data;
  },

  async delete(id: string): Promise<void> {
    await api.delete(`/stories/${id}`);
  },

  async updateCoverage(id: string, key: string, value: boolean): Promise<void> {
    await api.patch(`/stories/${id}/coverage`, { key, value });
  }
};
