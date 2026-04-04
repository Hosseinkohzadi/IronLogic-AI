namespace IronLogic.Infrastructure;

/// <summary>
///     Provides extension methods for string comparison and similarity calculations.
/// </summary>
public static class ExtensionMethods
{
    /// <summary>
    ///     Calculates the Dice similarity coefficient between two strings.
    ///     Returns a value between 0 (no similarity) and 1 (identical).
    /// </summary>
    /// <param name="source">The source string to compare.</param>
    /// <param name="target">The target string to compare against.</param>
    /// <returns>A double representing the similarity score between 0 and 1.</returns>
    public static double CalculateDiceSimilarity(string source, string target)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target)) return 0;

        source = source.ToLower().Trim();
        target = target.ToLower().Trim();

        if (source == target) return 1.0;

        // Create list of character pairs (Bigrams)
        var sPairs = GetBigrams(source);
        var tPairs = GetBigrams(target);

        int matches = 0;
        foreach (var sPair in sPairs.Where(sPair => tPairs.Contains(sPair)))
        {
            matches++;
            tPairs.Remove(sPair);
        }

        // Dice formula: (2 * number of matches) / (total of all pairs)
        return (2.0 * matches) / (sPairs.Count + GetBigrams(target).Count);
    }

    /// <summary>
    ///     Extracts bigrams (pairs of consecutive characters) from a string.
    /// </summary>
    /// <param name="str">The input string.</param>
    /// <returns>A list of bigram strings.</returns>
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