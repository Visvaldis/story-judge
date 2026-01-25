import { Page, Locator, expect } from '@playwright/test';

export class DashboardPage {
  readonly page: Page;
  readonly pageTitle: Locator;
  readonly createStoryButton: Locator;
  readonly storyList: Locator;
  readonly storyCards: Locator;
  readonly emptyState: Locator;

  constructor(page: Page) {
    this.page = page;
    this.pageTitle = page.locator('h1').first();
    // Target the header "New Story" button specifically, use first() to avoid multiple matches
    this.createStoryButton = page.locator('.dashboard-header a[href="/stories/new"]').first();
    this.storyList = page.locator('.stories-grid');
    this.storyCards = page.locator('.story-card');
    this.emptyState = page.locator('.empty-state');
  }

  async goto(): Promise<void> {
    await this.page.goto('/dashboard');
  }

  async expectToBeOnDashboard(): Promise<void> {
    await expect(this.page).toHaveURL(/\/dashboard/);
  }

  async getStoryCount(): Promise<number> {
    return await this.storyCards.count();
  }

  async clickCreateStory(): Promise<void> {
    await this.createStoryButton.click();
  }

  async clickStoryByTitle(title: string): Promise<void> {
    await this.page.locator(`text=${title}`).click();
  }

  async expectStoryWithTitle(title: string): Promise<void> {
    await expect(this.page.locator(`text=${title}`)).toBeVisible();
  }

  async expectEmptyState(): Promise<void> {
    await expect(this.emptyState).toBeVisible();
  }
}
