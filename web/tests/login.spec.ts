import { test, expect } from '@playwright/test';

test.describe('Login Component', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to the login page
    await page.goto('/login', { waitUntil: 'networkidle' });
  });

  test('should have a login form', async ({ page }) => {
    await expect(page.locator('form')).toBeVisible();

    const submitBtn = page.locator('button[type="submit"]');
    await expect(submitBtn).toContainText('Log In');
  });
});
