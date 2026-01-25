import { Page, Locator, expect } from '@playwright/test';

export class StoryEditorPage {
  readonly page: Page;
  readonly titleInput: Locator;
  readonly storyTypeSelect: Locator;
  readonly situationInput: Locator;
  readonly taskInput: Locator;
  readonly actionInput: Locator;
  readonly resultInput: Locator;
  readonly reflectionInput: Locator;
  readonly tagsInput: Locator;
  readonly visibilitySelect: Locator;
  readonly saveButton: Locator;
  readonly publishButton: Locator;
  readonly cancelButton: Locator;
  readonly successMessage: Locator;
  readonly errorMessage: Locator;

  constructor(page: Page) {
    this.page = page;
    // Using the actual IDs from StoryEditor.vue
    this.titleInput = page.locator('#title');
    this.storyTypeSelect = page.locator('#storyType');
    this.situationInput = page.locator('#situation');
    this.taskInput = page.locator('#task');
    this.actionInput = page.locator('#action');
    this.resultInput = page.locator('#result');
    this.reflectionInput = page.locator('#reflection');
    this.tagsInput = page.locator('.tags-input input');
    this.visibilitySelect = page.locator('#visibility');
    this.saveButton = page.locator('button[type="submit"]');
    this.publishButton = page.locator('button:has-text("Publish")');
    this.cancelButton = page.locator('button:has-text("Cancel"), a:has-text("Cancel")');
    this.successMessage = page.locator('[class*="success"], [role="alert"]:has-text("success")');
    this.errorMessage = page.locator('.error');
  }

  async goto(): Promise<void> {
    await this.page.goto('/stories/new');
  }

  async gotoEdit(storyId: string): Promise<void> {
    await this.page.goto(`/stories/${storyId}/edit`);
  }

  async fillStoryForm(story: {
    title: string;
    storyType?: string;
    situation: string;
    task: string;
    action: string;
    result: string;
    reflection?: string;
  }): Promise<void> {
    await this.titleInput.fill(story.title);

    if (story.storyType) {
      await this.storyTypeSelect.selectOption(story.storyType);
    }

    await this.situationInput.fill(story.situation);
    await this.taskInput.fill(story.task);
    await this.actionInput.fill(story.action);
    await this.resultInput.fill(story.result);

    if (story.reflection) {
      await this.reflectionInput.fill(story.reflection);
    }
  }

  async save(): Promise<void> {
    await this.saveButton.click();
  }

  async publish(visibility: 'Private' | 'Unlisted' | 'Public' = 'Public'): Promise<void> {
    if (await this.visibilitySelect.isVisible()) {
      await this.visibilitySelect.selectOption(visibility);
    }
    await this.publishButton.click();
  }

  async expectSaveSuccess(): Promise<void> {
    // Either show success message or redirect to story view
    const successIndicator = this.page.locator('[class*="success"], [role="alert"]');
    // Wait for either success message or URL change
    await Promise.race([
      expect(successIndicator).toBeVisible({ timeout: 5000 }),
      this.page.waitForURL(/\/stories\/[a-f0-9]+/, { timeout: 5000 }),
    ]);
  }

  async expectError(): Promise<void> {
    await expect(this.errorMessage).toBeVisible();
  }
}
