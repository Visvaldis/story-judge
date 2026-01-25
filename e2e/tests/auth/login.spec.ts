import { test, expect } from '@playwright/test';
import { LoginPage } from '../../helpers/page-objects/LoginPage';
import { registerUserViaApi, createTestUser } from '../../helpers/auth.helper';
import { clearAllCollections } from '../../helpers/db.helper';

test.describe('User Login', () => {
  let loginPage: LoginPage;
  let testUser: { email: string; password: string; displayName: string };

  test.beforeAll(async () => {
    await clearAllCollections();
    testUser = createTestUser();
    try {
      await registerUserViaApi(testUser);
    } catch {
      // User might already exist from previous run
    }
  });

  test.beforeEach(async ({ page }) => {
    loginPage = new LoginPage(page);
    // Navigate first, then clear localStorage
    await loginPage.goto();
    await page.evaluate(() => localStorage.clear());
    await page.reload();
  });

  test('should login with valid credentials', async ({ page }) => {
    await loginPage.login(testUser.email, testUser.password);

    await loginPage.expectSuccessfulLogin();

    // Verify token is stored
    const token = await page.evaluate(() => localStorage.getItem('token'));
    expect(token).toBeTruthy();
  });

  test('should not login with invalid credentials', async ({ page }) => {
    await loginPage.login(testUser.email, 'WrongPassword123');

    // Should stay on login page (not redirect to dashboard)
    await page.waitForTimeout(1000);
    await expect(page).toHaveURL(/\/login/);
  });

  test('should not login with non-existent user', async ({ page }) => {
    await loginPage.login('nonexistent@example.com', 'AnyPassword123');

    // Should stay on login page (not redirect to dashboard)
    await page.waitForTimeout(1000);
    await expect(page).toHaveURL(/\/login/);
  });

  test('should redirect to dashboard after successful login', async ({ page }) => {
    await loginPage.login(testUser.email, testUser.password);

    await expect(page).toHaveURL(/\/dashboard/);
  });

  test('should display Google OAuth button', async () => {
    await expect(loginPage.googleLoginButton).toBeVisible();
    await expect(loginPage.googleLoginButton).toContainText('Sign in with Google');
  });
});
