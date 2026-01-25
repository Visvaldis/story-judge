import { test, expect } from '@playwright/test';
import { StoryEditorPage } from '../../helpers/page-objects/StoryEditorPage';
import { StoryViewPage } from '../../helpers/page-objects/StoryViewPage';
import { registerUserViaApi, setAuthToken, createTestUser } from '../../helpers/auth.helper';
import { testStories } from '../../fixtures/stories';
import { clearAllCollections } from '../../helpers/db.helper';

const API_URL = process.env.API_URL || 'http://localhost:5107/api';

test.describe('Edit Story', () => {
  let storyEditorPage: StoryEditorPage;
  let storyViewPage: StoryViewPage;
  let authToken: string;
  let storyId: string;

  test.beforeAll(async () => {
    await clearAllCollections();
    const user = createTestUser();
    const result = await registerUserViaApi(user);
    authToken = result.token;

    // Create a story to edit
    const response = await fetch(`${API_URL}/stories`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${authToken}`,
      },
      body: JSON.stringify({
        ...testStories.draft,
        visibility: 'Private',
      }),
    });
    const story = await response.json();
    storyId = story.id;
  });

  test.beforeEach(async ({ page }) => {
    await setAuthToken(page, authToken);
    storyEditorPage = new StoryEditorPage(page);
    storyViewPage = new StoryViewPage(page);
  });

  test('should load existing story data in edit form', async ({ page }) => {
    await storyEditorPage.gotoEdit(storyId);

    await expect(storyEditorPage.titleInput).toHaveValue(testStories.draft.title);
    await expect(storyEditorPage.situationInput).toHaveValue(testStories.draft.situation);
    await expect(storyEditorPage.taskInput).toHaveValue(testStories.draft.task);
    await expect(storyEditorPage.actionInput).toHaveValue(testStories.draft.action);
    await expect(storyEditorPage.resultInput).toHaveValue(testStories.draft.result);
  });

  test('should update story title', async ({ page }) => {
    const newTitle = `Updated Title ${Date.now()}`;

    await storyEditorPage.gotoEdit(storyId);
    await storyEditorPage.titleInput.clear();
    await storyEditorPage.titleInput.fill(newTitle);
    await storyEditorPage.save();

    // Verify the story was updated
    await storyViewPage.goto(storyId);
    await storyViewPage.expectTitle(newTitle);
  });

  test('should update STAR sections', async ({ page }) => {
    const updatedSituation = 'Updated situation for testing purposes.';

    await storyEditorPage.gotoEdit(storyId);
    await storyEditorPage.situationInput.clear();
    await storyEditorPage.situationInput.fill(updatedSituation);
    await storyEditorPage.save();

    // Reload and verify
    await storyEditorPage.gotoEdit(storyId);
    await expect(storyEditorPage.situationInput).toHaveValue(updatedSituation);
  });

  test('should navigate from story view to edit', async ({ page }) => {
    await storyViewPage.goto(storyId);

    await storyViewPage.clickEdit();

    await expect(page).toHaveURL(new RegExp(`/stories/${storyId}/edit`));
  });
});
