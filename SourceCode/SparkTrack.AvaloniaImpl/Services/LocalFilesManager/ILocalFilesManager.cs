namespace SparkTrack.AvaloniaImpl.Services.LocalFilesManager;

using Enums;

public interface ILocalFilesManager
{
    /// <summary>
    /// Запускает средство для выбора файла для открытия
    /// </summary>
    /// <param name="suggestedFolderPath">Папка, которая будет открыта по умолчанию</param>
    /// <param name="fileTypes">Фильтры типов файлов</param>
    /// <returns>
    /// Путь выбранного файла, null - если средство было закрыто
    /// </returns>
    Task<string?> ChooseFileForOpenAsync(
        string? suggestedFolderPath = null,
        params EFileType[] fileTypes
    );
    
    /// <summary>
    /// Запускает средство для выбора набора файлов для открытия
    /// </summary>
    /// <param name="suggestedFolderPath">Папка, которая будет открыта по умолчанию</param>
    /// <param name="fileTypes">Фильтры типов файлов</param>
    /// <returns>
    /// Выбранные файлы
    /// </returns>
    Task<string[]> ChooseFilesForOpenAsync(
        string? suggestedFolderPath = null,
        params EFileType[] fileTypes
    );

    /// <summary>
    /// Запускает средство для выбора файла для сохранения
    /// </summary>
    /// <param name="suggestedFolderPath">Папка, которая будет открыта по умолчанию</param>
    /// <param name="suggestedFileName">Имя файла по умолчанию</param>
    /// <param name="fileTypes">Фильтры типов файлов</param>
    /// <returns>
    /// Путь выбранного файла, null - если средство было закрыто
    /// </returns>
    Task<string?> ChooseFileForSaveAsync(
        string? suggestedFolderPath = null,
        string? suggestedFileName = null,
        params EFileType[] fileTypes
    );

    /// <summary>
    /// Запускает средство для выбора папки
    /// </summary>
    /// <param name="suggestedFolderPath">Папка, которая будет открыта по умолчанию</param>
    /// <returns>
    /// Путь выбранной папки, null - если средство было закрыто
    /// </returns>
    Task<string?> ChooseDirectoryAsync(string? suggestedFolderPath = null);
}