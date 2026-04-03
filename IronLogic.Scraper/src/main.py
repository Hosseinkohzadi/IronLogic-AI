import asyncio
import json
import os
import re
from playwright.async_api import async_playwright
from crawler import get_exercise_links
from parser import parse_exercise_details

async def main():
    links_file = os.path.join("data", "links_list.txt")
    output_path = os.path.join("data", "exercises.json")
    os.makedirs("data", exist_ok=True)

    # Load and clean links
    if not os.path.exists(links_file) or os.path.getsize(links_file) == 0:
        links = await get_exercise_links()
    else:
        with open(links_file, "r", encoding="utf-8") as f:
            lines = f.readlines()
            links = [re.search(r'https://\S+', l).group(0) for l in lines if "https://" in l]

    # Resume management
    results = []
    scraped_urls = set()
    if os.path.exists(output_path):
        with open(output_path, "r", encoding="utf-8") as f:
            results = json.load(f)
            scraped_urls = {item['url'] for item in results if item['name'] not in ["Log In", "Unknown"]}

    pending = [l for l in links if l not in scraped_urls]
    print(f"🚀 Starting extraction for {len(pending)} pending exercises...")

    async with async_playwright() as p:
        context = await p.chromium.launch_persistent_context("user_data", headless=False)
        page = context.pages[0] if context.pages else await context.new_page()
        for i, link in enumerate(pending):
            print(f"🔄 ({i+1}/{len(pending)}) Scraping: {link}")
            data = await parse_exercise_details(page, link)
            if data and data['name'] not in ["Log In", "Unknown"]:
                results.append(data)
                if (i+1) % 5 == 0:
                    with open(output_path, "w", encoding="utf-8") as f: json.dump(results, f, indent=4, ensure_ascii=False)
            await asyncio.sleep(4)
        await context.close()

if __name__ == "__main__":
    asyncio.run(main())