namespace SparkTrack.AvaloniaImpl.Services.LocalFilesManager;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Enums;

public class LocalFilesManager : ILocalFilesManager
{
    #region Methods

    /// <inheritdoc />
    public async Task<string?> ChooseFileForSaveAsync(
        string? suggestedFolderPath,
        string? suggestedFileName,
        params EFileType[] fileTypes
    )
    {
        TopLevel topLevel = GetTopLevel();
        IStorageFolder? suggestedStartLocation = suggestedFolderPath is null
            ? null
            : await topLevel.StorageProvider.TryGetFolderFromPathAsync(suggestedFolderPath);

        FilePickerSaveOptions options = new()
        {
            FileTypeChoices = fileTypes.Select(DetermineFilePickerType).ToArray(),
            Title = "Сохранить",
            SuggestedStartLocation = suggestedStartLocation,
            SuggestedFileName = suggestedFileName
        };

        IStorageFile? file = await topLevel.StorageProvider.SaveFilePickerAsync(options);

        return file?.TryGetLocalPath() ?? string.Empty;
    }

    /// <inheritdoc />
    public async Task<string?> ChooseFileForOpenAsync(string? suggestedFolderPath = null, params EFileType[] fileTypes)
    {
        TopLevel topLevel = GetTopLevel();
        IStorageFolder? suggestedStartLocation = suggestedFolderPath is null
            ? null
            : await topLevel.StorageProvider.TryGetFolderFromPathAsync(suggestedFolderPath);

        FilePickerOpenOptions options = new()
        {
            AllowMultiple = false,
            FileTypeFilter = fileTypes.Select(DetermineFilePickerType).ToArray(),
            Title = "Открыть",
            SuggestedStartLocation = suggestedStartLocation,
        };

        IReadOnlyList<IStorageFile> file = await topLevel.StorageProvider.OpenFilePickerAsync(options);

        return file.FirstOrDefault()?.TryGetLocalPath() ?? string.Empty;
    }

    /// <inheritdoc />
    public async Task<string?> ChooseDirectoryAsync(string? suggestedFolderPath = null)
    {
        TopLevel topLevel = GetTopLevel();
        IStorageFolder? suggestedStartLocation = suggestedFolderPath is null
            ? null
            : await topLevel.StorageProvider.TryGetFolderFromPathAsync(suggestedFolderPath);

        FolderPickerOpenOptions options = new()
        {
            AllowMultiple = false,
            Title = "Выбор папки",
            SuggestedStartLocation = suggestedStartLocation,
        };

        IReadOnlyList<IStorageFolder> folder = await topLevel.StorageProvider.OpenFolderPickerAsync(options);

        return folder.FirstOrDefault()?.TryGetLocalPath();
    }

    private static TopLevel GetTopLevel()
    {
        TopLevel? topLevel = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
            ?.MainWindow;

        if (topLevel is null) throw new NullReferenceException("Cannot get TopLevel");

        return topLevel;
    }

    private static FilePickerFileType DetermineFilePickerType(EFileType fileType) => fileType switch
    {
        _ => FilePickerFileTypes.All,
    };

    #endregion
}