import json
import os
import re

def intelligent_clean():
    input_path = os.path.join( "exercises.json")
    output_path = os.path.join( "exercises_final.json")

    if not os.path.exists(input_path):
        print("❌ File not found!")
        return

    with open(input_path, "r", encoding="utf-8") as f:
        data = json.load(f)

    # 1. Blacklist based on names present in the file
    all_names = {item['name'].strip() for item in data if 'name' in item}
    
    # 2. Manually add items you mentioned (for 100% certainty)
    targets = [
        "Curtsy Lunge (Dumbbell)",
        "Wide-Elbow Triceps Press (Dumbbell)",
        "Zottman Curl (Dumbbell)"
    ]
    all_names.update(targets)

    # 3. Regex pattern to identify exercise names not in the list but with equipment format
    # This pattern finds any text ending with equipment in parentheses
    equipment_pattern = re.compile(r'.* \((Barbell|Dumbbell|Machine|Cable|Band|Kettlebell|Smith Machine|EZ bar|Suspension|Trap bar|Sled|Plate|Bodyweight)\)$')

    print(f"🧹 Cleaning {len(data)} exercises...")

    cleaned_count = 0
    for item in data:
        original_instrs = item.get('instructions', [])
        
        # Intelligent filtering:
        # If text is exactly in names or matches exercise name pattern (equipment), remove it
        new_instrs = [
            instr for instr in original_instrs 
            if instr.strip() not in all_names and not equipment_pattern.match(instr.strip())
        ]
        
        if len(new_instrs) != len(original_instrs):
            item['instructions'] = new_instrs
            cleaned_count += 1

    with open(output_path, "w", encoding="utf-8") as f:
        json.dump(data, f, indent=4, ensure_ascii=False)

    print(f"✅ Success! {cleaned_count} exercises were cleaned.")
    print(f"📂 Final file: {output_path}")

if __name__ == "__main__":
    intelligent_clean()