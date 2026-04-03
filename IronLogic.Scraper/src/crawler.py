import asyncio
import os
from playwright.async_api import async_playwright

async def get_exercise_links():
    exercise_links = []
    links_file = os.path.join("data", "links_list.txt")
    os.makedirs("data", exist_ok=True)

    async with async_playwright() as p:
        browser = await p.chromium.launch_persistent_context(
            "user_data", headless=False,
            user_agent="Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36"
        )
        page = browser.pages[0] if browser.pages else await browser.new_page()
        print("🔍 Accessing Hevy...")
        await page.goto("https://hevy.com/exercise", wait_until="networkidle")
        
        item_selector = "div.sc-e701c0fb-0.ipNNbu"
        for _ in range(25):  # Scroll to capture all 432 items
            items = await page.locator(item_selector).all()
            for item in items:
                try:
                    await item.click()
                    await asyncio.sleep(0.4)
                    if "/exercise/" in page.url and page.url not in exercise_links:
                        exercise_links.append(page.url)
                        if len(exercise_links) % 10 == 0:
                            with open(links_file, "w") as f:
                                f.write("\n".join(exercise_links))
                            print(f"💾 Captured {len(exercise_links)} links...")
                except: continue
            await page.mouse.wheel(0, 3500)
            await asyncio.sleep(1)

        await browser.close()
        return exercise_links