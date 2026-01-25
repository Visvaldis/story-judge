import { Page, expect } from '@playwright/test';

const API_URL = process.env.API_URL || 'http://localhost:5107/api';

export interface TestUser {
  email: string;
  password: string;
  displayName: string;
}

export async function registerUser(page: Page, user: TestUser): Promise<void> {
  await page.goto('/login');

  // Switch to Register tab
  await page.click('.tabs button:has-text("Register")');
  await expect(page.locator('#displayName')).toBeVisible();

  await page.fill('#displayName', user.displayName);
  await page.fill('#email', user.email);
  await page.fill('#password', user.password);

  await page.click('button[type="submit"]');

  // Wait for successful registration (redirect to dashboard)
  await expect(page).toHaveURL(/\/dashboard/);
}

export async function loginUser(page: Page, email: string, password: string): Promise<void> {
  await page.goto('/login');

  await page.fill('#email', email);
  await page.fill('#password', password);

  await page.click('button[type="submit"]');

  // Wait for successful login (redirect to dashboard)
  await expect(page).toHaveURL(/\/dashboard/);
}

export async function logoutUser(page: Page): Promise<void> {
  // Look for logout button/link in navbar
  const logoutButton = page.locator('button:has-text("Logout"), a:has-text("Logout"), [data-testid="logout"]');
  if (await logoutButton.isVisible()) {
    await logoutButton.click();
    await expect(page).toHaveURL(/\/(login|$)/);
  } else {
    // Clear localStorage as fallback
    await page.evaluate(() => localStorage.clear());
    await page.goto('/');
  }
}

export async function registerUserViaApi(user: TestUser): Promise<{ token: string; userId: string }> {
  const response = await fetch(`${API_URL}/auth/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(user),
  });

  if (!response.ok) {
    const error = await response.text();
    throw new Error(`Failed to register user: ${error}`);
  }

  const data = await response.json();
  return { token: data.token, userId: data.user.id };
}

export async function loginUserViaApi(email: string, password: string): Promise<{ token: string; userId: string }> {
  const response = await fetch(`${API_URL}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
  });

  if (!response.ok) {
    const error = await response.text();
    throw new Error(`Failed to login: ${error}`);
  }

  const data = await response.json();
  return { token: data.token, userId: data.user.id };
}

export async function setAuthToken(page: Page, token: string): Promise<void> {
  // Navigate to the base URL first to set localStorage on the correct origin
  await page.goto('/');
  await page.evaluate((authToken) => {
    localStorage.setItem('token', authToken);
  }, token);
}

export async function clearAuthToken(page: Page): Promise<void> {
  // Navigate to a page first to avoid SecurityError when accessing localStorage
  const url = page.url();
  if (!url || url === 'about:blank') {
    await page.goto('/');
  }
  await page.evaluate(() => localStorage.removeItem('token'));
}

export function generateTestEmail(): string {
  return `test.user.${Date.now()}@example.com`;
}

export function createTestUser(overrides?: Partial<TestUser>): TestUser {
  return {
    email: generateTestEmail(),
    password: 'TestPassword123',
    displayName: `Test User ${Date.now()}`,
    ...overrides,
  };
}
