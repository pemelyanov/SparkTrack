namespace Fanatiki.ZIP;

using System.IO.Compression;

public static class ZipUtils
{
    #region Methods

    public static void ExtractZipWithOverwrite(string zipPath, string extractPath)
    {
        // Проверка существования архива
        if (!File.Exists(zipPath))
            throw new FileNotFoundException($"ZIP-архив не найден: {zipPath}");

        // Создание целевой папки (если не существует)
        Directory.CreateDirectory(extractPath);

        // Открытие архива
        using ZipArchive? archive = ZipFile.OpenRead(zipPath);

        foreach (ZipArchiveEntry? entry in archive.Entries)
            try
            {
                // Полный путь к распакованному файлу
                string fullPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));

                // Проверка безопасности пути (защита от ZipSlip)
                if (!fullPath.StartsWith(extractPath, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Попытка распаковки вне целевой директории");

                // Для папок - создаем директорию
                if (string.IsNullOrEmpty(entry.Name))
                {
                    Directory.CreateDirectory(fullPath);
                    continue;
                }

                // Распаковка с перезаписью
                entry.ExtractToFile(fullPath, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при распаковке {entry.FullName}: {ex.Message}");
            }
    }

    #endregion
}