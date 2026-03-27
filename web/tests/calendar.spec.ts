import { test, expect } from '@playwright/test';

test.describe('IronLogic Dashboard - Calendar Component', () => {

    test.beforeEach(async ({ page }) => {
        // ⏱️ جادوی اول: Mock کردن زمان!
        // زمان مرورگر را روی یک تاریخ ثابت (مثلاً ۲۷ مارچ ۲۰۲۶) فریز می‌کنیم.
        // به این ترتیب، تقویم همیشه فکر می‌کند امروز این تاریخ است و تست‌ها فردا خراب نمی‌شوند.
        await page.clock.install({ time: new Date('2026-03-27T12:00:00') });

        // ورود به داشبورد
        await page.goto('http://localhost:4200/');
    });

    test('should render calendar correctly with "Today" indicator', async ({ page }) => {
        const calendar = page.locator('app-calendar');

        // مطمئن می‌شویم تقویم لود شده است
        await expect(calendar).toBeVisible();

        // بررسی می‌کنیم که هدر تقویم، ماه صحیح (که ماک کردیم) را نشان دهد
        await expect(calendar.locator('h3')).toContainText('March 2026');

        // 📸 جادوی دوم: توقف انیمیشن‌ها برای اسکرین‌شات
        // گزینه animations: 'disabled' باعث می‌شود افکت Pulse Glow در یک حالت ثابت فریز شود
        // تا عکس‌ها همیشه دقیقاً با هم تطابق داشته باشند.
        await expect(calendar).toHaveScreenshot('calendar-march-baseline.png', {
            animations: 'disabled',
            maxDiffPixels: 50
        });
    });

    test('should navigate to next and previous months correctly', async ({ page }) => {
        const calendar = page.locator('app-calendar');
        const headerTitle = calendar.locator('h3');

        // پیدا کردن دکمه‌های چپ و راست (بر اساس کلاس nav-btn)
        const prevBtn = calendar.locator('button.nav-btn').nth(0);
        const nextBtn = calendar.locator('button.nav-btn').nth(1);

        // ۱. تست ماه بعد
        await nextBtn.click();
        // بررسی تغییر متن هدر
        await expect(headerTitle).toContainText('April 2026');
        // گرفتن اسکرین‌شات از ماه جدید برای اطمینان از چیدمان صحیح روزها
        await expect(calendar).toHaveScreenshot('calendar-april.png', {
            animations: 'disabled'
        });

        // ۲. تست ماه قبل (برگشت به جای اول)
        await prevBtn.click();
        await expect(headerTitle).toContainText('March 2026');

        // ۳. تست ماه قبل‌تر
        await prevBtn.click();
        await expect(headerTitle).toContainText('February 2026');
    });

});
