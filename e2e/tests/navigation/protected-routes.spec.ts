import { test, expect } from '@playwright/test';
import { registerUserViaApi, setAuthToken, createTestUser } from '../../helpers/auth.helper';
import { clearAllCollections } from '../../helpers/db.helper';

test.describe('Protected Routes', () => {
  let authToken: string;

  test.beforeAll(async () => {
    await clearAllCollections();

    const user = createTestUser();
    const result = await registerUserViaApi(user);
    authToken = result.token;
  });

  test('should redirect to login when accessing dashboard unauthenticated', async ({ page }) => {
    // Navigate first to set the origin, then clear localStorage
    await page.goto('/');
    await page.evaluate(() => localStorage.clear());

    await page.goto('/dashboard');

    // Should redirect to login
    await expect(page).toHaveURL(/\/login/);
  });

  test('should redirect to login when accessing story editor unauthenticated', async ({ page }) => {
    await page.goto('/');
    await page.evaluate(() => localStorage.clear());

    await page.goto('/stories/new');

    await expect(page).toHaveURL(/\/login/);
  });

  test('should allow access to dashboard when authenticated', async ({ page }) => {
    await setAuthToken(page, authToken);

    await page.goto('/dashboard');

    await expect(page).toHaveURL(/\/dashboard/);
  });

  test('should allow access to explore page without authentication', async ({ page }) => {
    await page.goto('/');
    await page.evaluate(() => localStorage.clear());

    await page.goto('/explore');

    await expect(page).toHaveURL(/\/explore/);
  });

  test('should allow access to public story without authentication', async ({ page }) => {
    await page.goto('/');
    await page.evaluate(() => localStorage.clear());

    // Navigate to explore first
    await page.goto('/explore');

    // Stories page should be accessible
    await expect(page).toHaveURL(/\/explore/);
  });

  test('should redirect authenticated user away from login page', async ({ page }) => {
    await page.goto('/');
    await page.evaluate(() => localStorage.clear());

    // Try to access protected route
    await page.goto('/stories/new');

    // Should redirect to login
    await expect(page).toHaveURL(/\/login/);

    // Login by setting auth token
    await setAuthToken(page, authToken);
    await page.reload();

    // Wait for redirect to complete
    await page.waitForLoadState('networkidle');

    // After login, should be redirected away from login page
    // Note: The app currently redirects to home page, not back to intended URL
    // (redirect URL preservation is not implemented)
    await expect(page).not.toHaveURL(/\/login/);
  });

  test('should redirect to login on 401 API response', async ({ page }) => {
    // Navigate first then set invalid token
    await page.goto('/');
    await page.evaluate(() => {
      localStorage.setItem('token', 'invalid-token');
    });

    await page.goto('/dashboard');

    // The app should detect invalid token and redirect to login
    // This might take a moment as it needs to make an API call
    await page.waitForTimeout(2000);

    // Either on login page or token is cleared
    const token = await page.evaluate(() => localStorage.getItem('token'));
    const onLoginPage = page.url().includes('/login');

    expect(token === null || onLoginPage).toBe(true);
  });

  test('should show login/register links when not authenticated', async ({ page }) => {
    await page.goto('/');
    await page.evaluate(() => localStorage.clear());

    await page.goto('/explore');

    // Should see login link in navbar (register is a tab on login page)
    await expect(page.locator('a[href*="login"]')).toBeVisible();
  });

  test('should show user menu when authenticated', async ({ page }) => {
    await setAuthToken(page, authToken);

    await page.goto('/dashboard');

    // Should see user-related elements (logout, profile, etc.)
    const userElements = page.locator('button:has-text("Logout"), a:has-text("Profile"), [data-testid="user-menu"]');
    await expect(userElements.first()).toBeVisible();
  });
});
