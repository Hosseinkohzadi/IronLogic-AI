import { test, expect } from '@playwright/test';

test.describe('ImportWorkout Component', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to the import page
    await page.goto('/import', { waitUntil: 'networkidle' });
  });

  test('should simulate AI analysis process correctly', async ({ page }) => {
    // 1. Ensure the default text is in the textarea or fill it
    const textarea = page.locator('textarea');
    await textarea.fill('Test Hevy Data');

    // 2. Click the analyze button
    const analyzeBtn = page.locator('button', { hasText: 'Analyze & Extract Data' });
    await analyzeBtn.click();

    // 3. Check for the appearance of the Loading state
    const loadingText = page.locator('text=Running Semantic Kernel...');
    await expect(loadingText).toBeVisible();

    // 4. Wait for the processing result (Playwright will automatically wait for this text to appear)
    // We expect the calculated total volume to be displayed on the screen after 1.5 seconds
    const totalVolume = page.locator('text=13,460 lbs');
    await expect(totalVolume).toBeVisible({ timeout: 4000 }); // Waits for a maximum of 4 seconds
  });

  test('should not trigger analysis if rawLog is empty', async ({ page }) => {
    const textarea = page.locator('textarea');
    // Clear the text inside the box completely
    await textarea.fill('');

    // Check that the analyze button is disabled
    const analyzeBtn = page.locator('button', { hasText: 'Analyze & Extract Data' });
    await expect(analyzeBtn).toBeDisabled();
  });
});
