namespace SparkTrack.WebAPI.Services.Files;

using Core.Services.Files;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Upload;

public sealed class GoogleDriveFilesService(Func<Task<DriveService>> driveFactory)
    : IFilesService
{
    private const string FolderId = "1I3TWNzOXA6xXEhM9-TBB1dqITYwa53_3";

    public async Task<Guid> UploadAsync(Stream stream, CancellationToken cancellationToken)
    {
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

        var request = drive.Files.Create(
            metadata,
            stream,
            "application/octet-stream"
        );

        request.Fields = "id";
        request.ChunkSize = ResumableUpload.DefaultChunkSize;

        var progress = await request.UploadAsync(cancellationToken);

        if (progress.Status != UploadStatus.Completed)
        {
            throw new InvalidOperationException(
                $"Upload failed: {progress.Exception?.Message}"
            );
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
        var drive = await driveFactory();

        // Сначала ищем файл по имени в указанной папке
        var listRequest = drive.Files.List();
        var fileName = id.ToString();

        listRequest.Q = $"name = '{fileName}' and '{FolderId}' in parents and trashed = false";
        listRequest.Fields = "files(id, name, size)";
        listRequest.PageSize = 10;

        var searchResult = await listRequest.ExecuteAsync(cancellationToken);

        if (searchResult.Files == null || searchResult.Files.Count == 0)
        {
            throw new FileNotFoundException($"Файл с именем '{fileName}' не найден в папке {FolderId}");
        }
        
        var file = searchResult.Files[0];
        var fileId = file.Id;
        
        if (file.Size is { } size)
            contentLengthCallback(size);
        
        var downloadRequest = drive.Files.Get(fileId);
        await downloadRequest.DownloadAsync(stream, cancellationToken);
    }

    public async Task DeleteAsync(Guid id)
    {
        var drive = await driveFactory();
        await drive.Files.Delete(id.ToString()).ExecuteAsync();
    }
}