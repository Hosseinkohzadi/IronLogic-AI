import { test, expect } from '@playwright/test';

test.describe('IronLogic Dashboard - Training Duration', () => {

    // قبل از هر تست، وارد صفحه داشبورد می‌شویم
    test.beforeEach(async ({ page }) => {
        // آدرس لوکال‌هاست پروژه آنگولار شما
        await page.goto('http://localhost:4200/');
    });

    test('should render the chart and match initial visual state', async ({ page }) => {
        // پیدا کردن کامپوننت نمودار در صفحه
        const chartComponent = page.locator('app-training-duration');

        // مطمئن می‌شویم که کامپوننت لود شده است
        await expect(chartComponent).toBeVisible();

        // 📸 تست بصری (Visual Regression):
        // این خط از کامپوننت عکس می‌گیرد و با دفعات بعد مقایسه می‌کند
        await expect(chartComponent).toHaveScreenshot('training-duration-initial.png', {
            maxDiffPixels: 50 // حساسیت به تغییرات ریز (تا 50 پیکسل خطا مجاز است)
        });
    });

    test('should change metric to Volume and update the unit', async ({ page }) => {
        const chartComponent = page.locator('app-training-duration');

        // پیدا کردن دکمه 'volume' و کلیک روی آن
        await page.getByRole('button', { name: 'volume' }).click();

        // 🧪 تست منطق (E2E):
        // بررسی می‌کنیم که آیا واحد جلوی عدد درشت به 'LBS' تغییر کرده است؟
        await expect(chartComponent.locator('h4')).toContainText('LBS', { ignoreCase: true });

        // 📸 تست بصری دوم:
        // عکس از حالت Volume برای اطمینان از به هم نریختن استایل‌ها بعد از کلیک
        await expect(chartComponent).toHaveScreenshot('training-duration-volume.png');
    });
});
