import { test, expect } from '@playwright/test';
import { StoryViewPage } from '../../helpers/page-objects/StoryViewPage';
import { ReviewFormPage } from '../../helpers/page-objects/ReviewFormPage';
import { registerUserViaApi, setAuthToken, createTestUser } from '../../helpers/auth.helper';
import { generateUniqueStory } from '../../fixtures/stories';
import { clearAllCollections } from '../../helpers/db.helper';

const API_URL = process.env.API_URL || 'http://localhost:5107/api';

test.describe('Create Review', () => {
  let storyOwnerToken: string;
  let reviewerToken: string;
  let storyId: string;
  let storyTitle: string;

  test.beforeAll(async () => {
    await clearAllCollections();

    // Create story owner
    const owner = createTestUser();
    const ownerResult = await registerUserViaApi(owner);
    storyOwnerToken = ownerResult.token;

    // Create reviewer
    const reviewer = createTestUser();
    const reviewerResult = await registerUserViaApi(reviewer);
    reviewerToken = reviewerResult.token;

    // Create and publish a public story
    const story = generateUniqueStory('Behavioral');
    storyTitle = story.title;
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

  test('should submit a review with rubric scores', async ({ page }) => {
    await setAuthToken(page, reviewerToken);

    // Navigate to story
    const storyViewPage = new StoryViewPage(page);
    await storyViewPage.goto(storyId);

    // Wait for page to load
    await page.waitForLoadState('networkidle');

    // Verify the "Write a Review" button is visible (means we can review)
    await expect(storyViewPage.addReviewButton).toBeVisible({ timeout: 10000 });

    // Click add review button
    await storyViewPage.clickAddReview();

    // Wait for form to appear
    const reviewFormPage = new ReviewFormPage(page);
    await expect(reviewFormPage.submitButton).toBeVisible({ timeout: 5000 });

    // Fill review form - use default scores (3) which are already set
    // Just fill the feedback which is required
    await reviewFormPage.fillFeedback('This is a well-structured story with clear examples.');

    // Submit the review
    await reviewFormPage.submit();

    // Wait for form to close and page to update
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Verify review was submitted by reloading the page
    await storyViewPage.goto(storyId);
    await page.waitForLoadState('networkidle');

    const reviewCount = await storyViewPage.getReviewCount();
    expect(reviewCount).toBeGreaterThan(0);
  });

  test('should not allow reviewing own story', async ({ page }) => {
    await setAuthToken(page, storyOwnerToken);

    // Navigate to own story
    const storyViewPage = new StoryViewPage(page);
    await storyViewPage.goto(storyId);

    // The add review button should not be visible for own stories
    // or clicking it should show an error
    const addReviewButton = storyViewPage.addReviewButton;

    if (await addReviewButton.isVisible()) {
      await addReviewButton.click();

      // Should see error or the form should not appear
      const reviewFormPage = new ReviewFormPage(page);
      const errorVisible = await reviewFormPage.errorMessage.isVisible().catch(() => false);
      const formNotVisible = !(await reviewFormPage.submitButton.isVisible().catch(() => false));

      expect(errorVisible || formNotVisible).toBe(true);
    } else {
      // Button not visible is also acceptable
      expect(await addReviewButton.isVisible()).toBe(false);
    }
  });

  test('should require authentication to submit review', async ({ page }) => {
    // Navigate first then clear authentication (avoids SecurityError)
    await page.goto('/');
    await page.evaluate(() => localStorage.clear());

    // Navigate to story
    const storyViewPage = new StoryViewPage(page);
    await storyViewPage.goto(storyId);

    // Try to add review - should redirect to login or show auth required
    const addReviewButton = storyViewPage.addReviewButton;

    if (await addReviewButton.isVisible()) {
      await addReviewButton.click();

      // Should redirect to login
      await expect(page).toHaveURL(/\/login/);
    }
  });
});
