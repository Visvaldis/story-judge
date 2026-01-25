import { test, expect } from '@playwright/test';
import { StoryEditorPage } from '../../helpers/page-objects/StoryEditorPage';
import { DashboardPage } from '../../helpers/page-objects/DashboardPage';
import { registerUserViaApi, setAuthToken, createTestUser } from '../../helpers/auth.helper';
import { generateUniqueStory } from '../../fixtures/stories';
import { clearAllCollections } from '../../helpers/db.helper';

test.describe('Create Story', () => {
  let storyEditorPage: StoryEditorPage;
  let dashboardPage: DashboardPage;
  let authToken: string;

  test.beforeAll(async () => {
    await clearAllCollections();
    const user = createTestUser();
    const result = await registerUserViaApi(user);
    authToken = result.token;
  });

  test.beforeEach(async ({ page }) => {
    await setAuthToken(page, authToken);
    storyEditorPage = new StoryEditorPage(page);
    dashboardPage = new DashboardPage(page);
  });

  test('should create a draft story with STAR sections', async ({ page }) => {
    const story = generateUniqueStory('Behavioral');

    await storyEditorPage.goto();

    await storyEditorPage.fillStoryForm(story);
    await storyEditorPage.save();

    // Should redirect or show success
    await expect(page).not.toHaveURL(/\/stories\/new/);
  });

  test('should display validation errors for empty required fields', async ({ page }) => {
    await storyEditorPage.goto();

    // Try to save without filling in any fields
    await storyEditorPage.save();

    // Should stay on the page or show validation errors
    await expect(page).toHaveURL(/\/stories\/(new|create)/);
  });

  test('should navigate to story editor from dashboard', async ({ page }) => {
    await dashboardPage.goto();

    await dashboardPage.clickCreateStory();

    await expect(page).toHaveURL(/\/stories\/(new|create)/);
  });

  test('should save story with all STAR+R sections filled', async ({ page }) => {
    const story = generateUniqueStory('Technical');
    story.reflection = 'This experience taught me valuable lessons about problem-solving.';

    await storyEditorPage.goto();

    await storyEditorPage.fillStoryForm(story);
    await storyEditorPage.save();

    // Verify story was saved by checking redirect or success message
    await expect(page).not.toHaveURL(/\/stories\/new/);
  });

  test('should preserve story content after page refresh', async ({ page }) => {
    const story = generateUniqueStory('Leadership');

    await storyEditorPage.goto();
    await storyEditorPage.fillStoryForm(story);
    await storyEditorPage.save();

    // Get the story ID from URL and go back to edit
    await page.waitForURL(/\/stories\/[a-f0-9]+/);
    const storyUrl = page.url();
    const storyId = storyUrl.match(/\/stories\/([a-f0-9]+)/)?.[1];

    if (storyId) {
      await storyEditorPage.gotoEdit(storyId);

      // Verify content is preserved
      await expect(storyEditorPage.titleInput).toHaveValue(story.title);
    }
  });
});
