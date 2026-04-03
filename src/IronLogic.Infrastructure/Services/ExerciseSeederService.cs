using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using IronLogic.Domain.Entities;
using Exercise = IronLogic.Domain.Entities.Exercise;

namespace IronLogic.Infrastructure.Services;

public static class ExerciseSeederService
{
    public static async Task SeedAsync(AppDbContext context)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "data", "exercises_final.json");
        if (!File.Exists(filePath)) return;

        var jsonData = await File.ReadAllTextAsync(filePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var rawData = JsonSerializer.Deserialize<List<ExerciseJsonModel>>(jsonData, options);

        if (rawData == null || !rawData.Any()) return;

        // 🚀 مرحله ۱: اطمینان از وجود دیتای پایه (عضلات و تجهیزات)
        await EnsureBaseDataExistsAsync(context, rawData);

        // اگر تمرینات قبلاً اضافه شده‌اند، متوقف شو
        if (await context.Exercises.AnyAsync()) return;

        // 🚀 مرحله ۲: بارگذاری مجدد مپ‌ها بعد از ذخیره‌سازی مرحله اول
        var muscleMap = await context.Muscles.ToDictionaryAsync(m => m.Name.ToLower(), m => m);
        var equipmentMap = await context.Equipments.ToDictionaryAsync(e => e.Name.ToLower(), e => e.Id);

        // گرفتن IDهای پیش‌فرض برای اطمینان (Fallback)
        var defaultMuscleId = muscleMap.Values.First(x => x.Name.Equals("General", StringComparison.OrdinalIgnoreCase)).Id;
        var defaultEquipId = equipmentMap.Keys.Contains("none") ? equipmentMap["none"] : equipmentMap.Values.First();

        // 🚀 مرحله ۳: اضافه کردن تمرینات با هندل کردن مقادیر گمشده
        foreach (var item in rawData)
        {
            var exercise = new Exercise
            {
                Id = Guid.NewGuid(),
                Name = item.Name,
                Url = item.Url,
                ImagePath = item.Image_Path,
                Mechanics = item.Mechanics ?? "None",
                Instructions = item.Instructions != null ? string.Join("\n", item.Instructions) : "",
                SecondaryMuscles = new List<Muscle>()
            };

            // انتساب ایمن عضله اصلی
            var pMuscleName = (item.Primary_Muscle ?? "General").ToLower();
            exercise.PrimaryMuscleId = muscleMap.TryGetValue(pMuscleName, out var pm) ? pm.Id : defaultMuscleId;

            // انتساب ایمن تجهیزات
            var equipName = (item.Equipment ?? "None").ToLower();
            exercise.EquipmentId = equipmentMap.TryGetValue(equipName, out var eid) ? eid : defaultEquipId;

            // انتساب عضلات ثانویه
            if (!string.IsNullOrWhiteSpace(item.Secondary_Muscle) && item.Secondary_Muscle != "None")
            {
                var secondaryNames = item.Secondary_Muscle.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var sName in secondaryNames)
                {
                    if (muscleMap.TryGetValue(sName.Trim().ToLower(), out var secMuscle))
                    {
                        exercise.SecondaryMuscles.Add(secMuscle);
                    }
                }
            }

            context.Exercises.Add(exercise);
        }

        await context.SaveChangesAsync();
    }

    private static async Task EnsureBaseDataExistsAsync(AppDbContext context, List<ExerciseJsonModel> data)
    {
        // استخراج نام‌ها و اضافه کردن موجودیت "General" برای اطمینان
        var muscleNames = data.Select(x => x.Primary_Muscle)
            .Concat(data.SelectMany(x => (x.Secondary_Muscle ?? "").Split(',').Select(s => s.Trim())))
            .Append("General") // 🚀 تضمین وجود عضو پیش‌فرض
            .Where(x => !string.IsNullOrEmpty(x) && x != "None")
            .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var equipNames = data.Select(x => x.Equipment)
            .Append("None") // 🚀 تضمین وجود تجهیزات پیش‌فرض
            .Where(x => !string.IsNullOrEmpty(x))
            .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var existingMuscles = await context.Muscles.Select(m => m.Name.ToLower()).ToListAsync();
        foreach (var name in muscleNames.Where(n => !existingMuscles.Contains(n.ToLower())))
        {
            context.Muscles.Add(new Muscle { Id = Guid.NewGuid(), Name = name });
        }

        var existingEquip = await context.Equipments.Select(e => e.Name.ToLower()).ToListAsync();
        foreach (var name in equipNames.Where(n => !existingEquip.Contains(n.ToLower())))
        {
            context.Equipments.Add(new Equipment { Id = Guid.NewGuid(), Name = name });
        }

        await context.SaveChangesAsync(); // 🚀 بسیار مهم: ذخیره قبل از مرحله بعد
    }

    private class ExerciseJsonModel
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Image_Path { get; set; }
        public string Primary_Muscle { get; set; }
        public string Secondary_Muscle { get; set; }
        public string Equipment { get; set; }
        public string Mechanics { get; set; }
        public List<string> Instructions { get; set; }
    }
}