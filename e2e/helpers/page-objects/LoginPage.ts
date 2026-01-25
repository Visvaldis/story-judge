import { Page, Locator, expect } from '@playwright/test';

export class LoginPage {
  readonly page: Page;
  readonly signInTab: Locator;
  readonly registerTab: Locator;
  readonly emailInput: Locator;
  readonly passwordInput: Locator;
  readonly displayNameInput: Locator;
  readonly submitButton: Locator;
  readonly errorMessage: Locator;
  readonly googleLoginButton: Locator;

  constructor(page: Page) {
    this.page = page;
    this.signInTab = page.locator('.tabs button:has-text("Sign In")');
    this.registerTab = page.locator('.tabs button:has-text("Register")');
    this.emailInput = page.locator('#email');
    this.passwordInput = page.locator('#password');
    this.displayNameInput = page.locator('#displayName');
    this.submitButton = page.locator('button[type="submit"]');
    this.errorMessage = page.locator('.error-message');
    this.googleLoginButton = page.locator('.oauth-btn.google');
  }

  async goto(): Promise<void> {
    await this.page.goto('/login');
  }

  async switchToRegister(): Promise<void> {
    await this.registerTab.click();
    await expect(this.displayNameInput).toBeVisible();
  }

  async switchToSignIn(): Promise<void> {
    await this.signInTab.click();
    await expect(this.displayNameInput).not.toBeVisible();
  }

  async login(email: string, password: string): Promise<void> {
    await this.emailInput.fill(email);
    await this.passwordInput.fill(password);

    // Wait for either redirect or error message
    const responsePromise = this.page.waitForResponse(
      (response) => response.url().includes('/auth/login'),
      { timeout: 10000 }
    ).catch(() => null);

    await this.submitButton.click();
    await responsePromise;
  }

  async register(displayName: string, email: string, password: string): Promise<void> {
    await this.switchToRegister();
    await this.displayNameInput.fill(displayName);
    await this.emailInput.fill(email);
    await this.passwordInput.fill(password);
    await this.submitButton.click();
  }

  async expectSuccessfulLogin(): Promise<void> {
    await expect(this.page).toHaveURL(/\/dashboard/);
  }

  async expectError(message?: string): Promise<void> {
    // Wait for error message or stay on login page (no redirect)
    await expect(this.errorMessage).toBeVisible({ timeout: 10000 });
    if (message) {
      await expect(this.errorMessage).toContainText(message);
    }
  }

  async expectStaysOnLoginPage(): Promise<void> {
    // Alternative check - ensure we didn't redirect to dashboard
    await this.page.waitForTimeout(1000);
    await expect(this.page).toHaveURL(/\/login/);
  }
}
