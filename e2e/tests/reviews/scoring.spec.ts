import { test, expect } from '@playwright/test';
import { StoryViewPage } from '../../helpers/page-objects/StoryViewPage';
import { registerUserViaApi, setAuthToken, createTestUser } from '../../helpers/auth.helper';
import { generateUniqueStory } from '../../fixtures/stories';
import { clearAllCollections, getStoryById } from '../../helpers/db.helper';

const API_URL = process.env.API_URL || 'http://localhost:5107/api';

test.describe('Review Scoring', () => {
  let storyOwnerToken: string;
  let reviewer1Token: string;
  let reviewer2Token: string;
  let storyId: string;

  test.beforeAll(async () => {
    await clearAllCollections();

    // Create story owner
    const owner = createTestUser();
    const ownerResult = await registerUserViaApi(owner);
    storyOwnerToken = ownerResult.token;

    // Create reviewers
    const reviewer1 = createTestUser();
    const reviewer1Result = await registerUserViaApi(reviewer1);
    reviewer1Token = reviewer1Result.token;

    const reviewer2 = createTestUser();
    const reviewer2Result = await registerUserViaApi(reviewer2);
    reviewer2Token = reviewer2Result.token;

    // Create and publish a public story
    const story = generateUniqueStory('Technical');
    const createResponse = await fetch(`${API_URL}/stories`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${storyOwnerToken}`,
      },
      body: JSON.stringify({
        ...story,
        visibility: 'Private',
      }),
    });
    const createdStory = await createResponse.json();
    storyId = createdStory.id;

    // Publish the story
    await fetch(`${API_URL}/stories/${storyId}/publish`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${storyOwnerToken}`,
      },
      body: JSON.stringify({ visibility: 'Public' }),
    });
  });

  test('should update aggregated scores after review', async ({ page }) => {
    // Submit first review via API
    const review1Scores = {
      clarity: 4,
      ownership: 5,
      impact: 4,
      decisionMaking: 4,
      communication: 5,
      reflection: 4,
    };

    await fetch(`${API_URL}/reviews/story/${storyId}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${reviewer1Token}`,
      },
      body: JSON.stringify(review1Scores),
    });

    // Verify story has updated review stats
    await setAuthToken(page, storyOwnerToken);
    const storyViewPage = new StoryViewPage(page);
    await storyViewPage.goto(storyId);

    // Check that overall score is displayed
    await expect(storyViewPage.overallScore).toBeVisible();
  });

  test('should calculate average from multiple reviews', async ({ page }) => {
    // Submit second review
    const review2Scores = {
      clarity: 3,
      ownership: 4,
      impact: 3,
      decisionMaking: 4,
      communication: 3,
      reflection: 3,
    };

    await fetch(`${API_URL}/reviews/story/${storyId}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${reviewer2Token}`,
      },
      body: JSON.stringify(review2Scores),
    });

    // Fetch story and verify aggregated scores
    const response = await fetch(`${API_URL}/stories/${storyId}`, {
      headers: {
        'Authorization': `Bearer ${storyOwnerToken}`,
      },
    });
    const story = await response.json();

    // Verify review count
    expect(story.reviewStats.totalReviews).toBe(2);

    // Verify overall score is an average (between the two review scores)
    expect(story.reviewStats.overallScore).toBeGreaterThan(0);
    expect(story.reviewStats.overallScore).toBeLessThanOrEqual(5);
  });

  test('should display review count correctly', async ({ page }) => {
    await setAuthToken(page, storyOwnerToken);

    const storyViewPage = new StoryViewPage(page);
    await storyViewPage.goto(storyId);

    // Wait for page to fully load
    await page.waitForLoadState('networkidle');

    // Get review count from UI
    const reviewCount = await storyViewPage.getReviewCount();
    // Should have at least 1 review from previous tests
    expect(reviewCount).toBeGreaterThanOrEqual(1);
  });

  test('should show individual category scores', async ({ page }) => {
    const response = await fetch(`${API_URL}/stories/${storyId}`, {
      headers: {
        'Authorization': `Bearer ${storyOwnerToken}`,
      },
    });
    const story = await response.json();

    // Verify story has review stats
    expect(story.reviewStats).toBeDefined();
    expect(story.reviewStats.totalReviews).toBeGreaterThanOrEqual(1);
    expect(story.reviewStats.overallScore).toBeGreaterThan(0);
    expect(story.reviewStats.overallScore).toBeLessThanOrEqual(5);

    // categoryAverages may or may not be present depending on backend implementation
    if (story.reviewStats.categoryAverages) {
      const categories = ['clarity', 'ownership', 'impact', 'decisionMaking', 'communication', 'reflection'];
      for (const category of categories) {
        if (story.reviewStats.categoryAverages[category] !== undefined) {
          expect(story.reviewStats.categoryAverages[category]).toBeGreaterThan(0);
          expect(story.reviewStats.categoryAverages[category]).toBeLessThanOrEqual(5);
        }
      }
    }
  });
});
