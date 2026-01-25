import { Page, Locator, expect } from '@playwright/test';

export class RegisterPage {
  readonly page: Page;
  readonly displayNameInput: Locator;
  readonly emailInput: Locator;
  readonly passwordInput: Locator;
  readonly submitButton: Locator;
  readonly errorMessage: Locator;
  readonly loginLink: Locator;

  constructor(page: Page) {
    this.page = page;
    this.displayNameInput = page.locator('input[name="displayName"], input[placeholder*="name" i]');
    this.emailInput = page.locator('input[name="email"], input[type="email"]');
    this.passwordInput = page.locator('input[name="password"], input[type="password"]');
    this.submitButton = page.locator('button[type="submit"]');
    this.errorMessage = page.locator('[class*="error"], [role="alert"], .error-message');
    this.loginLink = page.locator('a[href*="login"]');
  }

  async goto(): Promise<void> {
    await this.page.goto('/register');
  }

  async register(displayName: string, email: string, password: string): Promise<void> {
    await this.displayNameInput.fill(displayName);
    await this.emailInput.fill(email);
    await this.passwordInput.fill(password);
    await this.submitButton.click();
  }

  async expectSuccessfulRegistration(): Promise<void> {
    // Should redirect to dashboard or login after registration
    await expect(this.page).toHaveURL(/\/(dashboard|login|$)/);
  }

  async expectError(message?: string): Promise<void> {
    await expect(this.errorMessage).toBeVisible();
    if (message) {
      await expect(this.errorMessage).toContainText(message);
    }
  }

  async goToLogin(): Promise<void> {
    await this.loginLink.click();
    await expect(this.page).toHaveURL(/\/login/);
  }
}
