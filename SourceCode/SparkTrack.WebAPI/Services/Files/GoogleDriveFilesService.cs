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

    public async Task<string> GetLinkAsync(Guid id)
    {
        s_logger.Info("Initializing drive");
        var drive = await driveFactory();

        var file = await GetFileByIdAsync(drive, id);

        if (string.IsNullOrEmpty(file.WebViewLink))
            throw new NotFoundException("Google Drive не вернул ссылку на файл");

        s_logger.Info("Link generated for file {id}", id);

        return file.WebViewLink;
    }

    public async Task<Guid> UploadAsync(Stream stream, long contentLength, string? extension, CancellationToken cancellationToken)
    {
        s_logger.Info("Initializing drive");
        var drive = await driveFactory();

        var fileId = Guid.NewGuid();

        var fileName = fileId.ToString();

        if (!string.IsNullOrEmpty(extension))
            fileName += $".{extension}";

        var metadata = new File
        {
            Name = fileName,
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

        const int mb = 1024 * 1024;
        request.Fields = "id";
        request.ChunkSize = 12 * mb;

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

        var file = await GetFileByIdAsync(drive, id);
        var fileId = file.Id;

        if (file.Size is { } size)
            contentLengthCallback(size);

        var downloadRequest = drive.Files.Get(fileId);

        var downloadStreamProxy = new ProgressWriteStream(stream, OnProgressChanged);

        s_logger.Info("Starting file download to stream: {fileName}", file.Name);
        await downloadRequest.DownloadAsync(downloadStreamProxy, cancellationToken);
        s_logger.Info("File download completed: {fileName}", file.Name);
        
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
    
    private async Task<File> GetFileByIdAsync(DriveService drive, Guid id)
    {
        var fileName = id.ToString();

        // Ищем файл в папке
        var listRequest = drive.Files.List();
        listRequest.Q =
            $"name contains '{fileName}' and '{m_folderId}' in parents and trashed = false";
        listRequest.Fields = "files(id, name, webViewLink)";
        listRequest.PageSize = 1;

        s_logger.Info("Searching file in folder: {id}", fileName);
        var searchResult = await listRequest.ExecuteAsync();

        if (searchResult.Files == null || searchResult.Files.Count == 0)
            throw new NotFoundException($"Файл '{fileName}' не найден в папке {m_folderId}");

        var file = searchResult.Files[0];
        return file;
    }
}