namespace SparkTrack.AvaloniaImpl;

using System.Security.Cryptography;

public static class Md5Helper
{
    /// <summary>
    /// Вычисляет MD5 файла и возвращает массив байт (16 байт)
    /// </summary>
    public static byte[] ComputeFileMd5(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Файл не найден", filePath);

        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var md5 = MD5.Create();
        return md5.ComputeHash(stream);
    }
    
    public static byte[] ComputeFileMd5(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var md5 = MD5.Create();
        return md5.ComputeHash(stream);
    }

    /// <summary>
    /// Проверяет, совпадает ли MD5 файла с переданным массивом
    /// </summary>
    public static bool VerifyFileMd5(string filePath, byte[] expectedMd5)
    {
        if (expectedMd5 == null || expectedMd5.Length != 16)
            throw new ArgumentException("MD5 должен быть массивом из 16 байт", nameof(expectedMd5));

        var actualMd5 = ComputeFileMd5(filePath);
        return AreEqual(actualMd5, expectedMd5);
    }

    /// <summary>
    /// Сравнивает два массива байт безопасно
    /// </summary>
    private static bool AreEqual(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
            return false;

        // Сравнение без раннего выхода, чтобы не дать подсказку по времени (хотя для файлов это не критично)
        var result = 0;
        for (int i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }

        return result == 0;
    }

    /// <summary>
    /// Вспомогательный метод для конвертации MD5 в hex (для логов или UI)
    /// </summary>
    public static string ToHex(byte[] md5)
    {
        return BitConverter.ToString(md5).Replace("-", "").ToLowerInvariant();
    }
}