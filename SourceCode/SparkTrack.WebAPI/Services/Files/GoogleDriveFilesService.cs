using NLog;
using SparkTrack.Core.Exceptions;
using ILogger = NLog.ILogger;

namespace SparkTrack.WebAPI.Services.Files;

using Core.Services.Files;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Upload;
using Streams;

public sealed class GoogleDriveFilesService(Func<Task<DriveService>> driveFactory, IConfiguration configuration)
    : IFilesService
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();

    private readonly string m_folderId =
        configuration.GetRequiredSection("Google").GetSection("FolderId").Get<string>()
        ?? throw new InvalidOperationException("Set FolderId in configuration");

    public async Task<Guid> UploadAsync(Stream stream, long contentLength,  CancellationToken cancellationToken)
    {
        s_logger.Info("Initializing drive");
        var drive = await driveFactory();

        var fileId = Guid.NewGuid();

        var metadata = new File
        {
            Name = fileId.ToString(),
            Parents =
            [
                m_folderId,
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

        request.ProgressChanged += OnProgressChanged;

        s_logger.Info("Starting file upload: {name}", metadata.Name);
        var result = await request.UploadAsync(cancellationToken);

        request.ProgressChanged -= OnProgressChanged;
        
        if (result.Status != UploadStatus.Completed)
        {
            cancellationToken.ThrowIfCancellationRequested();

            throw new InvalidOperationException(
                $"Upload failed: {result.Exception?.Message}"
            );
        }

        s_logger.Info("File uploaded: {name}", metadata.Name);

        return fileId;

        void OnProgressChanged(IUploadProgress progress)
        {
            s_logger.Info(
                "Uploading file to disk: '{id}. Uploaded: {bytes}/{total} ({percents:P})'",
                fileId,
                progress.BytesSent,
                contentLength,
                (float)progress.BytesSent / contentLength
            );
        }
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

        listRequest.Q = $"name = '{fileName}' and '{m_folderId}' in parents and trashed = false";
        listRequest.Fields = "files(id, name, size)";
        listRequest.PageSize = 10;

        s_logger.Info("Searching file in folder: {id}", fileName);
        var searchResult = await listRequest.ExecuteAsync(cancellationToken);

        if (searchResult.Files == null || searchResult.Files.Count == 0)
        {
            throw new NotFoundException($"Файл с именем '{fileName}' не найден в папке {m_folderId}");
        }

        var file = searchResult.Files[0];
        var fileId = file.Id;

        if (file.Size is { } size)
            contentLengthCallback(size);

        var downloadRequest = drive.Files.Get(fileId);

        var downloadStreamProxy = new ProgressWriteStream(stream, OnProgressChanged);

        s_logger.Info($"Starting file download to stream: {fileName}");
        await downloadRequest.DownloadAsync(downloadStreamProxy, cancellationToken);
        s_logger.Info($"File download completed: {fileName}");
        
        void OnProgressChanged(long bytes)
        {
            s_logger.Info(
                "Sending file to client: '{id}. Sent: {bytes}/{total} ({percents:P})'",
                fileId,
                bytes,
                file.Size,
                (float)bytes / file.Size
            );
        }
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