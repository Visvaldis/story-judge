import { Page, Locator, expect } from '@playwright/test';

export class StoryViewPage {
  readonly page: Page;
  readonly title: Locator;
  readonly authorName: Locator;
  readonly storyType: Locator;
  readonly situationSection: Locator;
  readonly taskSection: Locator;
  readonly actionSection: Locator;
  readonly resultSection: Locator;
  readonly reflectionSection: Locator;
  readonly editButton: Locator;
  readonly deleteButton: Locator;
  readonly reviewSection: Locator;
  readonly addReviewButton: Locator;
  readonly reviews: Locator;
  readonly shareButton: Locator;
  readonly overallScore: Locator;

  constructor(page: Page) {
    this.page = page;
    this.title = page.locator('h1');
    this.authorName = page.locator('[class*="author"], [data-testid="author"]');
    this.storyType = page.locator('[class*="story-type"], [data-testid="story-type"]');
    this.situationSection = page.locator('[data-testid="situation"], :text("Situation") + *');
    this.taskSection = page.locator('[data-testid="task"], :text("Task") + *');
    this.actionSection = page.locator('[data-testid="action"], :text("Action") + *');
    this.resultSection = page.locator('[data-testid="result"], :text("Result") + *');
    this.reflectionSection = page.locator('[data-testid="reflection"], :text("Reflection") + *');
    this.editButton = page.locator('a:has-text("Edit"), button:has-text("Edit")');
    this.deleteButton = page.locator('button:has-text("Delete")');
    this.reviewSection = page.locator('[class*="review"], [data-testid="reviews"]');
    this.addReviewButton = page.locator('button:has-text("Write a Review"), button:has-text("Add Review"), button:has-text("Review")');
    this.reviews = page.locator('[class*="review-card"], [data-testid="review"]');
    this.shareButton = page.locator('button:has-text("Share")');
    this.overallScore = page.locator('.review-stats .overall-score');
  }

  async goto(storyId: string): Promise<void> {
    await this.page.goto(`/stories/${storyId}`);
  }

  async gotoByShareToken(shareToken: string): Promise<void> {
    await this.page.goto(`/shared/${shareToken}`);
  }

  async expectTitle(title: string): Promise<void> {
    await expect(this.title).toContainText(title);
  }

  async expectAccessDenied(): Promise<void> {
    // The frontend shows "Story not found" for both 403 and 404 cases
    // Use getByText for text matching
    const errorIndicator = this.page.locator('.error').or(
      this.page.getByText('Story not found')
    ).or(
      this.page.getByText('Access denied')
    ).or(
      this.page.getByText('Not found')
    );
    await expect(errorIndicator.first()).toBeVisible();
  }

  async clickEdit(): Promise<void> {
    await this.editButton.click();
  }

  async clickAddReview(): Promise<void> {
    await this.addReviewButton.click();
  }

  async getReviewCount(): Promise<number> {
    return await this.reviews.count();
  }
}
