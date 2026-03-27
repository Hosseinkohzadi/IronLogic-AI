import { test, expect } from '@playwright/test';

test.describe('Register Component', () => {
    test.beforeEach(async ({ page }) => {
        // Navigate to the registration page
        await page.goto('/register', { waitUntil: 'networkidle' });
    });

    test('should render the registration form fields', async ({ page }) => {
        // Using placeholder or label instead of ID (more stable)
        await expect(page.getByPlaceholder(/name/i)).toBeVisible();
        await expect(page.getByPlaceholder(/email/i)).toBeVisible();
        await expect(page.locator('button[type="submit"]')).toBeVisible();
    });
});
