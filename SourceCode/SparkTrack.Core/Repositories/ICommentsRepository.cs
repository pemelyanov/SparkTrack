namespace SparkTrack.Core.Repositories;

using Shared.Data;
using Shared.Data.Entities;

public interface ICommentsRepository
{
    Task<IReadOnlyPagedData<Comment>> GetPageAsync(Guid featureId, PageQuery pageQuery);

    Task AddAsync(Guid featureId, Comment comment);

    Task<Guid?> GetAuthorIdAsync(Guid commentId);
    
    Task<Comment?> EditAsync(Comment comment);

    Task DeleteAsync(Guid id);
}