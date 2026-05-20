namespace SparkTrack.WebAPI.Services.Files;

using System.Collections.Concurrent;
using System.Threading.Channels;
using Core.Exceptions;
using Core.Services.Files;
using NLog;

public class GoogleDriveWithLocalDiskBufferingService(Func<IGoogleDriveFilesService> googleDriveFileServiceFactory)
    : BackgroundService, IFilesService
{
    private static readonly ILogger s_logger       = LogManager.GetCurrentClassLogger();
    private static readonly string  s_bufferFolder = Path.Combine(Path.GetTempPath(), "SparkFilesBuffer");

    private          Channel<BackgroundDriveUploadData>? m_eventsChannel;
    private readonly ConcurrentDictionary<Guid, string?> m_uploadingLocalFiles   = new();
    private readonly ConcurrentDictionary<Guid, int>     m_downloadingLocalFiles = new();

    public Task<string> GetLinkAsync(Guid id) => throw new NotImplementedException();

    public async Task<Guid> UploadAsync(
        Stream stream,
        long contentLength,
        string? extension,
        CancellationToken cancellationToken
    )
    {
        if (!HasEnoughSpaceInBuffer(contentLength))
        {
            s_logger.Warn("No enough space for buffering, uploading directly to google drive...");

            return await googleDriveFileServiceFactory()
                .UploadAsync(stream, contentLength, extension, cancellationToken);
        }

        Directory.CreateDirectory(s_bufferFolder);

        var fileId = Guid.CreateVersion7();

        var localFilePath = Path.Combine(s_bufferFolder, fileId.ToString());

        s_logger.Info("Buffering file at path: {path}", localFilePath);

        await using var fileStream = new FileStream(
            localFilePath,
            FileMode.OpenOrCreate,
            FileAccess.ReadWrite,
            FileShare.ReadWrite
        );

        var cts = new CancellationTokenSource();

        StartBackgroundDriveUpload(
            new BackgroundDriveUploadData(fileId, localFilePath, contentLength, extension, cts.Token)
        );

        try
        {
            await stream.CopyToAsync(fileStream, cancellationToken);
        }
        catch
        {
            await cts.CancelAsync();
        }
        
        return fileId;
    }

    public async Task DownloadAsync(
        Guid id,
        Stream stream,
        CancellationToken cancellationToken,
        Action<long> contentLengthCallback
    )
    {
        if (!m_uploadingLocalFiles.TryGetValue(id, out var localFilePath))
        {
            await googleDriveFileServiceFactory().DownloadAsync(id, stream, cancellationToken, contentLengthCallback);
            return;
        }

        if (!File.Exists(localFilePath)) throw new NotFoundException($"Файл {id} не найден");

        try
        {
            m_downloadingLocalFiles.AddOrUpdate(id, _ => 1, (_, value) => ++value);

            var fileInfo = new FileInfo(localFilePath);

            contentLengthCallback(fileInfo.Length);

            await using var fileStream = new FileStream(
                localFilePath,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.ReadWrite
            );

            await fileStream.CopyToAsync(stream, cancellationToken);
        }
        finally
        {
            if (m_downloadingLocalFiles.TryGetValue(id, out var downloaders))
            {
                if (downloaders <= 1)
                {
                    try
                    {
                        m_downloadingLocalFiles.TryRemove(id, out _);

                        if (!m_uploadingLocalFiles.ContainsKey(id) && File.Exists(localFilePath))
                            File.Delete(localFilePath);
                        else 
                            s_logger.Warn(
                                "Cannot delete buffer file. File is uploading to google drive or not exists. File path: {path}; File id: {id}",
                                localFilePath,
                                id
                            );
                    }
                    catch (Exception e)
                    {
                        s_logger.Error(e, "Error while deleting buffer file: {filePath}", localFilePath);
                    }
                }
                else
                    m_downloadingLocalFiles.TryUpdate(id, downloaders - 1, downloaders);
            }
        }
    }

    public Task DeleteAsync(Guid id) => throw new NotImplementedException();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        m_eventsChannel = Channel.CreateUnbounded<BackgroundDriveUploadData>();

        while (!stoppingToken.IsCancellationRequested)
        {
            var pendingEvent = await m_eventsChannel.Reader.ReadAsync(stoppingToken);

            _ = DoBackgroundDriveUpload(pendingEvent);
        }
    }

    private async Task DoBackgroundDriveUpload(BackgroundDriveUploadData data)
    {
        try
        {
            s_logger.Info(
                "Starting parallel file upload to google drive. File path: {path}; File id: {id}",
                data.LocalFilePath,
                data.FileId
            );

            m_uploadingLocalFiles.TryAdd(data.FileId, data.LocalFilePath);

            while (!File.Exists(data.LocalFilePath))
            {
                s_logger.Warn(
                    "Cannot find local file to upload, waiting for creation. File path: {path}",
                    data.LocalFilePath
                );
                await Task.Delay(100); // Ждем пока появится файл
            }

            await using var readStream = new FileStream(
                data.LocalFilePath,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.ReadWrite
            );

            await using var bufferStream = new BufferReadStream(readStream, data.ContentLength);

            s_logger.Info(
                "Uploading file to google drive. File path: {path}; File id: {id}",
                data.LocalFilePath,
                data.FileId
            );

            await googleDriveFileServiceFactory()
                .UploadAsync(
                    data.FileId,
                    bufferStream,
                    data.ContentLength,
                    data.Extension,
                    data.CancellationToken
                );

            s_logger.Info(
                "Parallel file upload completed. File path: {path}; File id: {id}",
                data.LocalFilePath,
                data.FileId
            );
        }
        catch (Exception e)
        {
            s_logger.Warn(e);
        }
        finally
        {
            if (!m_downloadingLocalFiles.ContainsKey(data.FileId) && File.Exists(data.LocalFilePath))
            {
                try
                {
                    s_logger.Info(
                        "Deleting buffer file. File path: {path}; File id: {id}",
                        data.LocalFilePath,
                        data.FileId
                    );
                    File.Delete(data.LocalFilePath);
                    m_uploadingLocalFiles.TryRemove(data.FileId, out _);
                }
                catch (Exception e)
                {
                    s_logger.Error(e, "Error while deleting buffer file: {filePath}", data.LocalFilePath);
                }
            }
            else
            {
                s_logger.Warn(
                    "Cannot delete buffer file. Uploaded file is downloading from local storage or not exists. File path: {path}; File id: {id}",
                    data.LocalFilePath,
                    data.FileId
                );
            }
        }
    }

    private bool HasEnoughSpaceInBuffer(long requiredSpace) => true;

    private void StartBackgroundDriveUpload(BackgroundDriveUploadData data)
    {
        if (m_eventsChannel is null) return;

        m_eventsChannel.Writer.WriteAsync(data);
    }

    record BackgroundDriveUploadData(
        Guid FileId,
        string LocalFilePath,
        long ContentLength,
        string? Extension,
        CancellationToken CancellationToken
    );

    private class BufferReadStream(FileStream fileStream, long totalFileLength) : Stream
    {
        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var readBytes = fileStream.Read(buffer, offset, count);

            if (readBytes > 0) return readBytes;

            if (fileStream.Position >= totalFileLength - 1) return 0;

            while (readBytes < 1)
            {
                Thread.Sleep(100);
                readBytes = fileStream.Read(buffer, offset, count);
            }

            return readBytes;
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => totalFileLength;

        public override long Position
        {
            get => fileStream.Position;
            set => fileStream.Position = value;
        }
    }
}