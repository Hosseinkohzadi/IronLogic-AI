import { test, expect } from '@playwright/test';

test.describe('Landing Component', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to the main page
    await page.goto('/');
  });

  test('should render the hero title', async ({ page }) => {
    const heading = page.locator('h1');
    await expect(heading).toContainText('Your Personal AI Coach');
  });
});
