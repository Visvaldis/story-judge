export interface User {
  id: string;
  email: string;
  displayName: string;
  username?: string;
  avatarUrl?: string;
  bio?: string;
  roleLevel: string;
  reputationScore: number;
  createdAt: string;
}

export interface CoverageItem {
  key: string;
  label: string;
  detected: boolean;
  manualOverride?: boolean;
}

export interface ReviewStats {
  totalReviews: number;
  overallScore: number;
  categoryAverages: Record<string, number>;
}

export interface Story {
  id: string;
  authorId: string;
  authorName: string;
  authorAvatarUrl?: string;
  title: string;
  storyType: string;
  situation: string;
  task: string;
  action: string;
  result: string;
  reflection?: string;
  tags: string[];
  visibility: string;
  status: string;
  shareToken?: string;
  coverage: CoverageItem[];
  reviewStats: ReviewStats;
  publishedAt?: string;
  createdAt: string;
  updatedAt: string;
}

export interface StoryListItem {
  id: string;
  authorId: string;
  authorName: string;
  authorAvatarUrl?: string;
  title: string;
  storyType: string;
  tags: string[];
  visibility: string;
  status: string;
  reviewStats: ReviewStats;
  publishedAt?: string;
  createdAt: string;
}

export interface ReviewScores {
  clarity: number;
  ownership: number;
  impact: number;
  decisionMaking: number;
  communication: number;
  reflection: number;
  technicalDepth?: number;
}

export interface Review {
  id: string;
  storyId: string;
  reviewerId: string;
  reviewerName: string;
  reviewerAvatarUrl?: string;
  scores: ReviewScores;
  overallScore: number;
  feedback?: string;
  strengths: string[];
  improvements: string[];
  createdAt: string;
  updatedAt: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface CreateStoryRequest {
  title: string;
  storyType: StoryType;
  situation: string;
  task: string;
  action: string;
  result: string;
  reflection?: string;
  tags?: string[];
  visibility: Visibility;
}

export interface UpdateStoryRequest {
  title?: string;
  storyType?: StoryType;
  situation?: string;
  task?: string;
  action?: string;
  result?: string;
  reflection?: string;
  tags?: string[];
  visibility?: Visibility;
}

export interface CreateReviewRequest {
  clarity: number;
  ownership: number;
  impact: number;
  decisionMaking: number;
  communication: number;
  reflection: number;
  technicalDepth?: number;
  feedback?: string;
  strengths?: string[];
  improvements?: string[];
}

export type StoryType = 'Behavioral' | 'Technical' | 'Leadership' | 'Conflict' | 'Failure' | 'Achievement';
export type Visibility = 'Private' | 'Unlisted' | 'Public';
export type StoryStatus = 'Draft' | 'Published';

export const STORY_TYPES: StoryType[] = ['Behavioral', 'Technical', 'Leadership', 'Conflict', 'Failure', 'Achievement'];
export const VISIBILITY_OPTIONS: Visibility[] = ['Private', 'Unlisted', 'Public'];

export const REVIEW_CATEGORIES = [
  { key: 'clarity', label: 'Clarity', weight: 1.0 },
  { key: 'ownership', label: 'Ownership', weight: 1.2 },
  { key: 'impact', label: 'Impact', weight: 1.2 },
  { key: 'decisionMaking', label: 'Decision Making', weight: 1.0 },
  { key: 'communication', label: 'Communication', weight: 1.0 },
  { key: 'reflection', label: 'Reflection', weight: 0.8 },
  { key: 'technicalDepth', label: 'Technical Depth', weight: 0.6, optional: true }
];
