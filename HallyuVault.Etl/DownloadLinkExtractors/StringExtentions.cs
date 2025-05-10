namespace HallyuVault.Etl.DownloadLinkExtractors;

public static class StringExtentions
{
    public static string Replace(this string input, string[] oldValues, string[] newValues)
    {
        if (oldValues.Length != newValues.Length)
            throw new ArgumentException("Arrays must be of equal length.");

        for (int i = 0; i < oldValues.Length; i++)
        {
            input = input.Replace(oldValues[i], newValues[i]);
        }
        return input;
    }

    private static readonly HashSet<string> ZipExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".zip", ".rar", ".7z", ".tar", ".gz", ".xz", ".bz2", ".tgz"
    };

    public static bool IsZipped(this string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        string extension = Path.GetExtension(fileName);
        return ZipExtensions.Contains(extension);
    }
}