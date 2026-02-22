namespace SparkTrack.Core.Services.Files;

public interface IFilesService
{
    // TODO: Убрать extension и поменять API при доработке метода сохранения файлов. Сейчас временно добавляем только указание расширения для удобства поиска в хранилище
    Task<Guid> UploadAsync(Stream stream, long contentLength, string? extension, CancellationToken cancellationToken);

    Task DownloadAsync(Guid id, Stream stream, CancellationToken cancellationToken, Action<long> contentLengthCallback);

    Task DeleteAsync(Guid id);
}