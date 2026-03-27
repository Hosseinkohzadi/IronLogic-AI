import { test, expect } from '@playwright/test';

test.describe('App Root Component', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to the application's root
    await page.goto('/');
  });

  test('should load the app successfully', async ({ page }) => {
    // Check that the page has loaded without issues (e.g., the body tag is visible)
    const body = page.locator('body');
    await expect(body).toBeVisible();
  });

  test('should contain a router-outlet for routing', async ({ page }) => {
    // The router-outlet tag usually has no physical or visible style on the page (it's just a placeholder)
    // So, instead of toBeVisible, we use toBeAttached to just check for its existence in the DOM
    const routerOutlet = page.locator('router-outlet');
    await expect(routerOutlet).toBeAttached();
  });
});
