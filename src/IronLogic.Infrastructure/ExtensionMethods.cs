namespace IronLogic.Infrastructure;

public static class ExtensionMethods
{
    public static double CalculateDiceSimilarity(string source, string target)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target)) return 0;
    
        source = source.ToLower().Trim();
        target = target.ToLower().Trim();

        if (source == target) return 1.0;

        // ایجاد لیست جفت‌حروف (Bigrams)
        var sPairs = GetBigrams(source);
        var tPairs = GetBigrams(target);

        int matches = 0;
        foreach (var sPair in sPairs.Where(sPair => tPairs.Contains(sPair)))
        {
            matches++;
            tPairs.Remove(sPair);
        }

        // فرمول Dice: (2 * تعداد اشتراکات) / (مجموع کل جفت‌ها)
        return (2.0 * matches) / (sPairs.Count + GetBigrams(target).Count);
    }

    private static List<string> GetBigrams(string str)
    {
        var bigrams = new List<string>();
        for (int i = 0; i < str.Length - 1; i++)
        {
            bigrams.Add(str.Substring(i, 2));
        }
        return bigrams;
    }
}