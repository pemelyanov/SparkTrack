namespace SparkTrack.Core.Services.Archive;

using Shared.Enums;

public interface IArchiveService<in TId>
{
    Task ArchiveAsync(TId id, EArchiveSource source, bool executingInExternalTransaction = false);
}