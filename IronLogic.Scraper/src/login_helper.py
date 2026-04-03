import asyncio
from playwright.async_api import async_playwright

async def run():
    """
    Opens a browser window for manual login. Session data is saved to 'user_data'.
    """
    async with async_playwright() as p:
        context = await p.chromium.launch_persistent_context(
            "user_data",
            headless=False,
            user_agent="Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36"
        )
        page = await context.new_page()
        await page.goto("https://hevy.com/login")

        print("✅ Please log in manually and navigate to the 'Exercises' page.")
        print("✅ Once the list is visible, return here and press Enter.")

        await asyncio.to_thread(input, "Press Enter after you are logged in...")
        await context.close()

if __name__ == "__main__":
    asyncio.run(run())