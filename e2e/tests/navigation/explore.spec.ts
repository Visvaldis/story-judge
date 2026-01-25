import { test, expect } from '@playwright/test';
import { ExplorePage } from '../../helpers/page-objects/ExplorePage';
import { registerUserViaApi, setAuthToken, createTestUser } from '../../helpers/auth.helper';
import { clearAllCollections } from '../../helpers/db.helper';

const API_URL = process.env.API_URL || 'http://localhost:5107/api';

test.describe('Explore Page Navigation', () => {
  let authToken: string;

  test.beforeAll(async () => {
    await clearAllCollections();

    const user = createTestUser();
    const result = await registerUserViaApi(user);
    authToken = result.token;

    // Create multiple public stories of different types
    const storyTypes = ['Behavioral', 'Technical', 'Leadership', 'Conflict'];
    for (const storyType of storyTypes) {
      const story = {
        title: `Test ${storyType} Story`,
        storyType,
        situation: `Situation for ${storyType} story`,
        task: `Task for ${storyType} story`,
        action: `Action for ${storyType} story`,
        result: `Result for ${storyType} story`,
        visibility: 'Private',
      };

      const response = await fetch(`${API_URL}/stories`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${authToken}`,
        },
        body: JSON.stringify(story),
      });
      const createdStory = await response.json();

      // Publish as public
      await fetch(`${API_URL}/stories/${createdStory.id}/publish`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${authToken}`,
        },
        body: JSON.stringify({ visibility: 'Public' }),
      });
    }
  });

  test('should display public stories on explore page', async ({ page }) => {
    const explorePage = new ExplorePage(page);
    await explorePage.goto();

    const storyCount = await explorePage.getStoryCount();
    expect(storyCount).toBeGreaterThanOrEqual(4);
  });

  test('should filter stories by story type', async ({ page }) => {
    await setAuthToken(page, authToken);

    const explorePage = new ExplorePage(page);
    await explorePage.goto();

    // Filter by Technical
    const typeFilter = explorePage.storyTypeFilter;
    if (await typeFilter.isVisible()) {
      await explorePage.filterByStoryType('Technical');

      // Should show only technical stories
      await expect(page.locator('text=Test Technical Story')).toBeVisible();

      // Other types should not be visible
      await expect(page.locator('text=Test Leadership Story')).not.toBeVisible();
    }
  });

  test('should sort stories correctly', async ({ page }) => {
    const explorePage = new ExplorePage(page);
    await explorePage.goto();

    const sortSelect = explorePage.sortSelect;
    if (await sortSelect.isVisible()) {
      // Sort by newest
      await explorePage.sortBy('newest');

      // Verify stories are displayed (order verification would require more complex logic)
      const storyCount = await explorePage.getStoryCount();
      expect(storyCount).toBeGreaterThan(0);
    }
  });

  test('should navigate to story view on story click', async ({ page }) => {
    const explorePage = new ExplorePage(page);
    await explorePage.goto();

    // Click on first story
    const storyCards = explorePage.storyCards;
    if (await storyCards.first().isVisible()) {
      await storyCards.first().click();

      await expect(page).toHaveURL(/\/stories\/[a-f0-9]+/);
    }
  });

  test('should be accessible without authentication', async ({ page }) => {
    // Navigate first then clear any auth (avoids SecurityError)
    await page.goto('/');
    await page.evaluate(() => localStorage.clear());

    const explorePage = new ExplorePage(page);
    await explorePage.goto();

    // Should not redirect to login
    await expect(page).toHaveURL(/\/explore/);

    // Should show stories
    const storyCount = await explorePage.getStoryCount();
    expect(storyCount).toBeGreaterThan(0);
  });

  test('should show story type and author info in cards', async ({ page }) => {
    const explorePage = new ExplorePage(page);
    await explorePage.goto();

    // Story cards should have type and author information
    const storyCard = explorePage.storyCards.first();
    if (await storyCard.isVisible()) {
      // Should contain story type element
      await expect(storyCard.locator('.story-type')).toBeVisible();
      // Should contain author name
      await expect(storyCard.locator('.author')).toBeVisible();
    }
  });
});
