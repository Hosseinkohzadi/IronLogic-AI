import { test, expect } from '@playwright/test';

test.describe('Forgot Component', () => {
    test.beforeEach(async ({ page }) => {
        // Navigate to the forgot password page
        await page.goto('/forgot', { waitUntil: 'networkidle' });
    });

    test('should render the reset password button', async ({ page }) => {
        const submitBtn = page.locator('button[type="submit"]');
        await expect(submitBtn).toContainText('Reset Password'); // Or whatever text is on your button
    });
});
