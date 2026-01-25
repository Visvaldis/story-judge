import { Page, Locator, expect } from '@playwright/test';

export class ReviewFormPage {
  readonly page: Page;
  readonly clarityScore: Locator;
  readonly ownershipScore: Locator;
  readonly impactScore: Locator;
  readonly decisionMakingScore: Locator;
  readonly communicationScore: Locator;
  readonly reflectionScore: Locator;
  readonly technicalDepthScore: Locator;
  readonly feedbackInput: Locator;
  readonly strengthsInput: Locator;
  readonly improvementsInput: Locator;
  readonly submitButton: Locator;
  readonly cancelButton: Locator;
  readonly errorMessage: Locator;
  readonly successMessage: Locator;

  constructor(page: Page) {
    this.page = page;
    // The form uses .score-input divs with labels and range inputs
    // Use nth-child to find specific score inputs
    this.clarityScore = page.locator('.score-input').filter({ hasText: 'Clarity' }).locator('input[type="range"]');
    this.ownershipScore = page.locator('.score-input').filter({ hasText: 'Ownership' }).locator('input[type="range"]');
    this.impactScore = page.locator('.score-input').filter({ hasText: 'Impact' }).locator('input[type="range"]');
    this.decisionMakingScore = page.locator('.score-input').filter({ hasText: 'Decision Making' }).locator('input[type="range"]');
    this.communicationScore = page.locator('.score-input').filter({ hasText: 'Communication' }).locator('input[type="range"]');
    this.reflectionScore = page.locator('.score-input').filter({ hasText: 'Reflection' }).locator('input[type="range"]');
    this.technicalDepthScore = page.locator('.score-input').filter({ hasText: 'Technical Depth' }).locator('input[type="range"]');
    this.feedbackInput = page.locator('.review-form textarea');
    this.strengthsInput = page.locator('[placeholder*="strength" i], input:below(:text("Strengths"))');
    this.improvementsInput = page.locator('[placeholder*="improvement" i], input:below(:text("Improvements"))');
    this.submitButton = page.locator('.review-form button[type="submit"]');
    this.cancelButton = page.locator('.review-form button:has-text("Cancel")');
    this.errorMessage = page.locator('.review-form .error, [role="alert"]:has-text("error")');
    this.successMessage = page.locator('.review-form .success, [role="alert"]:has-text("success")');
  }

  async fillScores(scores: {
    clarity?: number;
    ownership?: number;
    impact?: number;
    decisionMaking?: number;
    communication?: number;
    reflection?: number;
    technicalDepth?: number;
  }): Promise<void> {
    // Form defaults all scores to 3, so we only need to set if different
    if (scores.clarity !== undefined) await this.setScore(this.clarityScore, scores.clarity);
    if (scores.ownership !== undefined) await this.setScore(this.ownershipScore, scores.ownership);
    if (scores.impact !== undefined) await this.setScore(this.impactScore, scores.impact);
    if (scores.decisionMaking !== undefined) await this.setScore(this.decisionMakingScore, scores.decisionMaking);
    if (scores.communication !== undefined) await this.setScore(this.communicationScore, scores.communication);
    if (scores.reflection !== undefined) await this.setScore(this.reflectionScore, scores.reflection);
    if (scores.technicalDepth !== undefined) await this.setScore(this.technicalDepthScore, scores.technicalDepth);
  }

  private async setScore(locator: Locator, value: number): Promise<void> {
    // For range inputs, we need to use fill() to set the value
    if (await locator.isVisible()) {
      await locator.fill(value.toString());
    }
  }

  async fillFeedback(feedback: string): Promise<void> {
    await this.feedbackInput.fill(feedback);
  }

  async submit(): Promise<void> {
    await this.submitButton.click();
  }

  async expectSuccess(): Promise<void> {
    await expect(this.successMessage).toBeVisible();
  }

  async expectError(message?: string): Promise<void> {
    await expect(this.errorMessage).toBeVisible();
    if (message) {
      await expect(this.errorMessage).toContainText(message);
    }
  }
}
