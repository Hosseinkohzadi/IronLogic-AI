import os
import requests
import re
import asyncio
from playwright.async_api import Page

async def parse_exercise_details(page: Page, url: str):
    try:
        await page.goto(url, wait_until="load", timeout=60000)
        await asyncio.sleep(2)
        if "login" in page.url: return {"name": "Log In"}

        # Extract name from h2
        exercise_name = await page.evaluate("() => Array.from(document.querySelectorAll('h2')).pop()?.innerText")
        name = exercise_name.strip() if exercise_name else "Unknown"

        # Extract instructions
        instructions = []
        try:
            how_to_tab = page.get_by_text("How to", exact=True)
            if await how_to_tab.is_visible():
                await how_to_tab.click()
                await asyncio.sleep(1)
                instructions = await page.evaluate("() => Array.from(document.querySelectorAll('p')).map(p => p.innerText.trim()).filter(t => t.length > 20 && !t.includes(':'))")
        except: pass

        # Smart media download
        media_url = await page.evaluate("() => document.querySelector('video source')?.src || document.querySelector('img[src*=\"exercise\"]')?.src")
        local_path = await download_media(name, media_url) if media_url else "General"

        return {
            "name": name, "url": url, "image_path": local_path,
            "primary_muscle": await extract_label(page, "Primary Muscle Group"),
            "secondary_muscle": await extract_label(page, "Secondary Muscle Group"),
            "equipment": await extract_label(page, "Equipment"),
            "mechanics": await extract_label(page, "Mechanics"),
            "instructions": instructions
        }
    except Exception as e:
        print(f"❌ Error on {url}: {e}")
        return None

async def download_media(name, url):
    folder = os.path.join("data", "images")
    os.makedirs(folder, exist_ok=True)
    clean_name = re.sub(r'[\\/*?:"<>|]', "", name).replace(" ", "_").lower()
    ext = ".mp4" if ".mp4" in url.lower() else ".gif" if ".gif" in url.lower() else ".webp"
    filepath = os.path.join(folder, f"{clean_name}{ext}")

    # 🚀 Check file existence before downloading
    if os.path.exists(filepath):
        return f"assets/exercises/{clean_name}{ext}"

    try:
        r = requests.get(url, stream=True, timeout=15)
        if r.status_code == 200:
            with open(filepath, 'wb') as f:
                for chunk in r.iter_content(1024): f.write(chunk)
            return f"assets/exercises/{clean_name}{ext}"
    except: pass
    return "General"

async def extract_label(page, label):
    return await page.evaluate(f"""() => {{
        const target = Array.from(document.querySelectorAll('p')).find(p => p.innerText.includes('{label}'));
        return target?.nextElementSibling ? target.nextElementSibling.innerText.trim() : 'None';
    }}""")