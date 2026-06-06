namespace SparkTrack.Core.Client;

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

public class Paths
{
    public static string ApplicationData { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SparkTrack"
    );

    public static string NormalizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));

        // Forbidden characters for Windows/Linux/macOS
        char[] invalidChars = Path.GetInvalidFileNameChars()
            .Concat(
                [
                    ':', '*', '?', '"', '<', '>', '|', '/', '\\',
                ]
            )
            .Distinct()
            .ToArray();

        // Replace invalid chars with underscore
        var normalized = new StringBuilder(fileName.Length);
        foreach (char c in fileName) normalized.Append(invalidChars.Contains(c) ? '_' : c);

        // Normalize unicode (e.g. accented chars → base chars)
        string result = normalized.ToString()
            .Normalize(NormalizationForm.FormD);

        result = new string(
                result
                    .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    .ToArray()
            )
            .Normalize(NormalizationForm.FormC);

        // Trim dots and spaces (forbidden at start/end on Windows)
        result = result.Trim('.', ' ');

        // Collapse multiple underscores/spaces
        result = Regex.Replace(result, @"[_\s]{2,}", "_");

        // Windows reserved names: CON, PRN, AUX, NUL, COM1–COM9, LPT1–LPT9
        if (Regex.IsMatch(result, @"^(CON|PRN|AUX|NUL|COM\d|LPT\d)(\.|$)", RegexOptions.IgnoreCase))
            result = "_" + result;

        // Limit length (255 bytes is the max on most FS)
        byte[] bytes = Encoding.UTF8.GetBytes(result);
        if (bytes.Length > 255) result = Encoding.UTF8.GetString(bytes[..255]).TrimEnd('\uFFFD');

        // Final fallback
        if (string.IsNullOrWhiteSpace(result))
            result = "file";

        return result;
    }
}