namespace SparkTrack.Core.Services.Archive;

using Repositories;
using Shared.Enums;

public class UserArchiveService(IUsersRepository usersRepository) : IUserArchiveService
{
    public Task ArchiveAsync(Guid id, EArchiveSource source, bool executingInExternalTransaction = false) =>
        usersRepository.SetArchiveStatus(id, true, source);
}