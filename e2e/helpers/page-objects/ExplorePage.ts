import { Page, Locator, expect } from '@playwright/test';

export class ExplorePage {
  readonly page: Page;
  readonly pageTitle: Locator;
  readonly storyCards: Locator;
  readonly storyTypeFilter: Locator;
  readonly sortSelect: Locator;
  readonly searchInput: Locator;
  readonly emptyState: Locator;
  readonly pagination: Locator;
  readonly nextPageButton: Locator;
  readonly prevPageButton: Locator;

  constructor(page: Page) {
    this.page = page;
    this.pageTitle = page.locator('h1, h2').first();
    this.storyCards = page.locator('[class*="story-card"], [class*="story-item"], [data-testid="story-card"]');
    this.storyTypeFilter = page.locator('select[name="storyType"], [data-testid="story-type-filter"]');
    this.sortSelect = page.locator('select[name="sort"], [data-testid="sort"]');
    this.searchInput = page.locator('input[type="search"], input[placeholder*="search" i]');
    this.emptyState = page.locator('[class*="empty"], :text("No stories")');
    this.pagination = page.locator('[class*="pagination"], nav[aria-label*="pagination"]');
    this.nextPageButton = page.locator('button:has-text("Next"), a:has-text("Next"), [aria-label="Next page"]');
    this.prevPageButton = page.locator('button:has-text("Previous"), a:has-text("Previous"), [aria-label="Previous page"]');
  }

  async goto(): Promise<void> {
    await this.page.goto('/explore');
    // Wait for the page to finish loading stories
    await this.page.waitForLoadState('networkidle');
    // Give time for Vue to render
    await this.page.waitForTimeout(500);
  }

  async getStoryCount(): Promise<number> {
    return await this.storyCards.count();
  }

  async filterByStoryType(type: string): Promise<void> {
    await this.storyTypeFilter.selectOption(type);
    // Wait for stories to reload
    await this.page.waitForLoadState('networkidle');
  }

  async sortBy(sortOption: string): Promise<void> {
    await this.sortSelect.selectOption(sortOption);
    await this.page.waitForLoadState('networkidle');
  }

  async search(query: string): Promise<void> {
    await this.searchInput.fill(query);
    await this.searchInput.press('Enter');
    await this.page.waitForLoadState('networkidle');
  }

  async clickStoryByTitle(title: string): Promise<void> {
    await this.page.locator(`text=${title}`).click();
  }

  async goToNextPage(): Promise<void> {
    await this.nextPageButton.click();
    await this.page.waitForLoadState('networkidle');
  }

  async goToPreviousPage(): Promise<void> {
    await this.prevPageButton.click();
    await this.page.waitForLoadState('networkidle');
  }

  async expectEmptyState(): Promise<void> {
    await expect(this.emptyState).toBeVisible();
  }

  async expectStoryVisible(title: string): Promise<void> {
    await expect(this.page.locator(`text=${title}`)).toBeVisible();
  }
}
