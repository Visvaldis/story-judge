import { test, expect } from '@playwright/test';
import { StoryViewPage } from '../../helpers/page-objects/StoryViewPage';
import { ExplorePage } from '../../helpers/page-objects/ExplorePage';
import { registerUserViaApi, setAuthToken, createTestUser, clearAuthToken } from '../../helpers/auth.helper';
import { generateUniqueStory } from '../../fixtures/stories';
import { clearAllCollections } from '../../helpers/db.helper';

const API_URL = process.env.API_URL || 'http://localhost:5107/api';

test.describe('Story Visibility', () => {
  let ownerToken: string;
  let otherUserToken: string;

  test.beforeAll(async () => {
    await clearAllCollections();

    // Create story owner
    const owner = createTestUser();
    const ownerResult = await registerUserViaApi(owner);
    ownerToken = ownerResult.token;

    // Create another user
    const otherUser = createTestUser();
    const otherResult = await registerUserViaApi(otherUser);
    otherUserToken = otherResult.token;
  });

  test('should allow viewing public story without authentication', async ({ page }) => {
    // Create and publish a public story
    const story = generateUniqueStory('Behavioral');
    const createResponse = await fetch(`${API_URL}/stories`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${ownerToken}`,
      },
      body: JSON.stringify({
        ...story,
        visibility: 'Private',
      }),
    });
    const createdStory = await createResponse.json();

    // Publish as Public
    await fetch(`${API_URL}/stories/${createdStory.id}/publish`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${ownerToken}`,
      },
      body: JSON.stringify({ visibility: 'Public' }),
    });

    // Clear any authentication (clearAuthToken navigates first to avoid SecurityError)
    await clearAuthToken(page);

    // View story without being logged in
    const storyViewPage = new StoryViewPage(page);
    await storyViewPage.goto(createdStory.id);

    await storyViewPage.expectTitle(story.title);
  });

  test('should deny access to private story for non-owner', async ({ page }) => {
    // Create a private story
    const story = generateUniqueStory('Leadership');
    const createResponse = await fetch(`${API_URL}/stories`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${ownerToken}`,
      },
      body: JSON.stringify({
        ...story,
        visibility: 'Private',
      }),
    });
    const createdStory = await createResponse.json();

    // Try to access as another user
    await setAuthToken(page, otherUserToken);

    const storyViewPage = new StoryViewPage(page);
    await storyViewPage.goto(createdStory.id);

    // Wait for the page to finish loading
    await page.waitForLoadState('networkidle');

    // The key security check: verify the private story content is NOT visible
    // The story title should NOT be displayed (this is what matters for security)
    await expect(page.locator(`h1:has-text("${story.title}")`)).not.toBeVisible({ timeout: 3000 });

    // Also verify the story's unique situation text is not visible
    await expect(page.getByText(story.situation)).not.toBeVisible();
  });

  test('should allow owner to view their private story', async ({ page }) => {
    // Create a private story
    const story = generateUniqueStory('Technical');
    const createResponse = await fetch(`${API_URL}/stories`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${ownerToken}`,
      },
      body: JSON.stringify({
        ...story,
        visibility: 'Private',
      }),
    });
    const createdStory = await createResponse.json();

    // Access as owner
    await setAuthToken(page, ownerToken);

    const storyViewPage = new StoryViewPage(page);
    await storyViewPage.goto(createdStory.id);

    await storyViewPage.expectTitle(story.title);
  });

  test('should allow access to unlisted story via share token', async ({ page }) => {
    // Create and publish as unlisted
    const story = generateUniqueStory('Conflict');
    const createResponse = await fetch(`${API_URL}/stories`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${ownerToken}`,
      },
      body: JSON.stringify({
        ...story,
        visibility: 'Private',
      }),
    });
    const createdStory = await createResponse.json();

    // Publish as Unlisted
    const publishResponse = await fetch(`${API_URL}/stories/${createdStory.id}/publish`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${ownerToken}`,
      },
      body: JSON.stringify({ visibility: 'Unlisted' }),
    });
    const publishedStory = await publishResponse.json();

    // Clear authentication (navigate first to avoid SecurityError)
    await page.goto('/');
    await page.evaluate(() => localStorage.clear());

    // Access via share token
    const storyViewPage = new StoryViewPage(page);
    await storyViewPage.gotoByShareToken(publishedStory.shareToken);

    await storyViewPage.expectTitle(story.title);
  });

  test('should not show unlisted story on explore page', async ({ page }) => {
    // Create and publish as unlisted
    const story = generateUniqueStory('Achievement');
    const createResponse = await fetch(`${API_URL}/stories`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${ownerToken}`,
      },
      body: JSON.stringify({
        ...story,
        visibility: 'Private',
      }),
    });
    const createdStory = await createResponse.json();

    // Publish as Unlisted
    await fetch(`${API_URL}/stories/${createdStory.id}/publish`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${ownerToken}`,
      },
      body: JSON.stringify({ visibility: 'Unlisted' }),
    });

    // Check explore page
    const explorePage = new ExplorePage(page);
    await explorePage.goto();

    // Unlisted story should not appear
    await expect(page.locator(`text=${story.title}`)).not.toBeVisible();
  });
});
