using NLog;
using SparkTrack.Core.Exceptions;
using ILogger = NLog.ILogger;

namespace SparkTrack.WebAPI.Services.Files;

using Core.Services.Files;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Upload;

public sealed class GoogleDriveFilesService(Func<Task<DriveService>> driveFactory)
    : IFilesService
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();
    private const           string  FolderId = "1I3TWNzOXA6xXEhM9-TBB1dqITYwa53_3"; //TODO: вынести в appsettings.json

    public async Task<Guid> UploadAsync(Stream stream, CancellationToken cancellationToken)
    {
        s_logger.Info("Initializing drive");
        var drive = await driveFactory();

        var fileId = Guid.NewGuid();

        var metadata = new File
        {
            Name = fileId.ToString(),
            Parents =
            [
                FolderId,
            ]
        };
        
        s_logger.Info("Creating file on drive: {name}", metadata.Name);
        
        var request = drive.Files.Create(
            metadata,
            stream,
            "application/octet-stream"
        );

        request.Fields = "id";
        request.ChunkSize = ResumableUpload.DefaultChunkSize;

        s_logger.Info("Starting file upload: {name}", metadata.Name);
        var progress = await request.UploadAsync(cancellationToken);

        if (progress.Status != UploadStatus.Completed)
        {
            throw new InvalidOperationException(
                $"Upload failed: {progress.Exception?.Message}"
            );
        }
        
        s_logger.Info("File uploaded: {name}", metadata.Name);

        return fileId;
    }

    public async Task DownloadAsync(
        Guid id,
        Stream stream,
        CancellationToken cancellationToken,
        Action<long> contentLengthCallback
    )
    {
        s_logger.Info("Initializing drive");
        var drive = await driveFactory();

        // Сначала ищем файл по имени в указанной папке
        var listRequest = drive.Files.List();
        var fileName = id.ToString();

        listRequest.Q = $"name = '{fileName}' and '{FolderId}' in parents and trashed = false";
        listRequest.Fields = "files(id, name, size)";
        listRequest.PageSize = 10;

        s_logger.Info("Searching file in folder: {id}", fileName);
        var searchResult = await listRequest.ExecuteAsync(cancellationToken);

        if (searchResult.Files == null || searchResult.Files.Count == 0)
        {
            throw new NotFoundException($"Файл с именем '{fileName}' не найден в папке {FolderId}");
        }
        
        var file = searchResult.Files[0];
        var fileId = file.Id;
        
        if (file.Size is { } size)
            contentLengthCallback(size);
        
        
        var downloadRequest = drive.Files.Get(fileId);
        s_logger.Info($"Starting file download to stream: {fileName}");
        await downloadRequest.DownloadAsync(stream, cancellationToken);
        s_logger.Info($"File download completed: {fileName}");
    }

    public async Task DeleteAsync(Guid id)
    {
        s_logger.Info("Initializing drive");
        var drive = await driveFactory();
        
        s_logger.Info("Deleting file from drive: {name}", id);
        // TODO: Добавить поиск по имени
        await drive.Files.Delete(id.ToString()).ExecuteAsync();
    }
}