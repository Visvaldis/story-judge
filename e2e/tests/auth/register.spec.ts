import { test, expect } from '@playwright/test';
import { LoginPage } from '../../helpers/page-objects/LoginPage';
import { generateUniqueUser } from '../../fixtures/users';
import { clearAllCollections } from '../../helpers/db.helper';

test.describe('User Registration', () => {
  let loginPage: LoginPage;

  test.beforeEach(async ({ page }) => {
    loginPage = new LoginPage(page);
  });

  test.beforeAll(async () => {
    await clearAllCollections();
  });

  test('should register a new user successfully', async ({ page }) => {
    const user = generateUniqueUser('register');
    await loginPage.goto();

    await loginPage.register(user.displayName, user.email, user.password);

    await loginPage.expectSuccessfulLogin();
  });

  test('should show error for duplicate email', async ({ page }) => {
    const user = generateUniqueUser('duplicate');

    // Register first user
    await loginPage.goto();
    await loginPage.register(user.displayName, user.email, user.password);
    await loginPage.expectSuccessfulLogin();

    // Clear local storage and try to register again with same email
    await page.evaluate(() => localStorage.clear());
    await loginPage.goto();
    await loginPage.register('Another User', user.email, user.password);

    await loginPage.expectError();
  });

  test('should switch between Sign In and Register tabs', async ({ page }) => {
    await loginPage.goto();

    // Default is Sign In
    await expect(loginPage.displayNameInput).not.toBeVisible();

    // Switch to Register
    await loginPage.switchToRegister();
    await expect(loginPage.displayNameInput).toBeVisible();

    // Switch back to Sign In
    await loginPage.switchToSignIn();
    await expect(loginPage.displayNameInput).not.toBeVisible();
  });
});
