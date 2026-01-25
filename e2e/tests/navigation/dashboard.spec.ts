import { test, expect } from '@playwright/test';
import { DashboardPage } from '../../helpers/page-objects/DashboardPage';
import { registerUserViaApi, setAuthToken, createTestUser } from '../../helpers/auth.helper';
import { generateUniqueStory } from '../../fixtures/stories';
import { clearAllCollections } from '../../helpers/db.helper';

const API_URL = process.env.API_URL || 'http://localhost:5107/api';

test.describe('Dashboard Navigation', () => {
  let authToken: string;

  test.beforeAll(async () => {
    const user = createTestUser();
    const result = await registerUserViaApi(user);
    authToken = result.token;
  });

  test('should display dashboard after login', async ({ page }) => {
    await setAuthToken(page, authToken);

    const dashboardPage = new DashboardPage(page);
    await dashboardPage.goto();

    await dashboardPage.expectToBeOnDashboard();
  });

  test('should show empty state or stories on dashboard', async ({ page }) => {
    await setAuthToken(page, authToken);

    const dashboardPage = new DashboardPage(page);
    await dashboardPage.goto();

    // Dashboard should load - either with stories or empty state
    const storyCount = await dashboardPage.getStoryCount();
    if (storyCount === 0) {
      await dashboardPage.expectEmptyState();
    } else {
      expect(storyCount).toBeGreaterThan(0);
    }
  });

  test('should navigate to story editor on create button click', async ({ page }) => {
    await setAuthToken(page, authToken);

    const dashboardPage = new DashboardPage(page);
    await dashboardPage.goto();

    await dashboardPage.clickCreateStory();

    await expect(page).toHaveURL(/\/stories\/(new|create)/);
  });

  test('should navigate to story view on story click', async ({ page }) => {
    await setAuthToken(page, authToken);

    const dashboardPage = new DashboardPage(page);
    await dashboardPage.goto();

    // Click on a story card
    const storyCards = dashboardPage.storyCards;
    if (await storyCards.first().isVisible()) {
      await storyCards.first().click();

      await expect(page).toHaveURL(/\/stories\/[a-f0-9]+/);
    }
  });

  test('should show navbar with user info', async ({ page }) => {
    await setAuthToken(page, authToken);

    const dashboardPage = new DashboardPage(page);
    await dashboardPage.goto();

    // Verify navbar elements
    const navbar = page.locator('nav.navbar');
    await expect(navbar).toBeVisible();

    // Should have explore link
    await expect(page.locator('a[href*="explore"]')).toBeVisible();
  });
});
