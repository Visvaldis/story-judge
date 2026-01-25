import { test, expect } from '@playwright/test';
import { StoryEditorPage } from '../../helpers/page-objects/StoryEditorPage';
import { StoryViewPage } from '../../helpers/page-objects/StoryViewPage';
import { ExplorePage } from '../../helpers/page-objects/ExplorePage';
import { registerUserViaApi, setAuthToken, createTestUser } from '../../helpers/auth.helper';
import { generateUniqueStory } from '../../fixtures/stories';
import { clearAllCollections } from '../../helpers/db.helper';

const API_URL = process.env.API_URL || 'http://localhost:5107/api';

test.describe('Publish Story', () => {
  let authToken: string;

  test.beforeAll(async () => {
    await clearAllCollections();
    const user = createTestUser();
    const result = await registerUserViaApi(user);
    authToken = result.token;
  });

  test('should publish story as Public', async ({ page }) => {
    await setAuthToken(page, authToken);

    // Create a draft story via API
    const story = generateUniqueStory('Behavioral');
    const createResponse = await fetch(`${API_URL}/stories`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authToken}`,
      },
      body: JSON.stringify({
        ...story,
        visibility: 'Private',
      }),
    });
    const createdStory = await createResponse.json();

    // Navigate to story editor to publish
    const storyEditorPage = new StoryEditorPage(page);
    await storyEditorPage.gotoEdit(createdStory.id);

    // Set visibility to Public
    await storyEditorPage.visibilitySelect.selectOption('Public');

    // Click Publish button
    const publishButton = page.locator('button:has-text("Publish")');
    await expect(publishButton).toBeVisible();
    await publishButton.click();

    // Wait for navigation to story view
    await page.waitForURL(/\/stories\/[a-f0-9]+$/);

    // Verify story is now published by checking the status badge on story view
    await expect(page.locator('.story-status.published')).toBeVisible();
    await expect(page.locator('text=Published')).toBeVisible();
  });

  test('should publish story as Unlisted with share token', async ({ page }) => {
    await setAuthToken(page, authToken);

    // Create and publish as Unlisted via API
    const story = generateUniqueStory('Technical');
    const createResponse = await fetch(`${API_URL}/stories`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authToken}`,
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
        'Authorization': `Bearer ${authToken}`,
      },
      body: JSON.stringify({ visibility: 'Unlisted' }),
    });
    const publishedStory = await publishResponse.json();

    // Verify share token exists
    expect(publishedStory.shareToken).toBeTruthy();

    // Verify story is NOT visible on explore page
    const explorePage = new ExplorePage(page);
    await explorePage.goto();

    await expect(page.locator(`text=${story.title}`)).not.toBeVisible();

    // But accessible via share token
    const storyViewPage = new StoryViewPage(page);
    await storyViewPage.gotoByShareToken(publishedStory.shareToken);

    await storyViewPage.expectTitle(story.title);
  });

  test('should not show draft stories on explore page', async ({ page }) => {
    await setAuthToken(page, authToken);

    // Create a draft story
    const story = generateUniqueStory('Conflict');
    await fetch(`${API_URL}/stories`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authToken}`,
      },
      body: JSON.stringify({
        ...story,
        visibility: 'Private',
      }),
    });

    // Check explore page - draft should not be visible
    const explorePage = new ExplorePage(page);
    await explorePage.goto();

    await expect(page.locator(`text=${story.title}`)).not.toBeVisible();
  });
});
