import { test, expect } from '@playwright/test';

test.describe('IronLogic Dashboard - Calendar Component', () => {

    test.beforeEach(async ({ page }) => {
        // First Magic Trick: Mocking Time!
        // We freeze the browser's time to a fixed date (e.g., March 27, 2026).
        // This way, the calendar always thinks today is this date, and the tests won't fail tomorrow.
        await page.clock.install({ time: new Date('2026-03-27T12:00:00') });

        // Navigate to the dashboard
        await page.goto('/dashboard', { waitUntil: 'networkidle' });
    });

    test('should render calendar correctly with "Today" indicator', async ({ page }) => {
        const calendar = page.locator('app-calendar');

        // Ensure the calendar is loaded
        await expect(calendar).toBeVisible();

        // Check that the calendar header shows the correct month (the one we mocked)
        await expect(calendar.locator('h3')).toContainText('March 2026');

        // Second Magic Trick: Disabling animations for screenshots
        // The `animations: 'disabled'` option freezes the Pulse Glow effect in a static state
        // so that the screenshots always match exactly.
        await expect(calendar).toHaveScreenshot('calendar-march-baseline.png', {
            animations: 'disabled',
            maxDiffPixels: 50
        });
    });

    test('should navigate to next and previous months correctly', async ({ page }) => {
        await page.goto('/dashboard', { waitUntil: 'networkidle' });
        const calendar = page.locator('app-calendar');

        // Find buttons by order (left and right)
        const prevBtn = calendar.locator('button').first();
        const nextBtn = calendar.locator('button').nth(1);

        await nextBtn.click();
        await expect(calendar.locator('h3')).toContainText('April 2026');

        await prevBtn.click();
        await expect(calendar.locator('h3')).toContainText('March 2026');
    });

});
