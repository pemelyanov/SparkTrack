namespace SparkTrack.Core.Repositories;

using Shared.Data;
using Shared.Data.Edit;
using Shared.Data.Entities;

public interface ICommentsRepository
{
    Task<IReadOnlyPagedData<Comment>> GetPageAsync(Guid featureId, PageQuery pageQuery);

    Task AddAsync(Comment comment);

    Task<Guid?> GetAuthorIdAsync(Guid commentId);
    
    Task<Comment?> EditAsync(CommentEdit commentEdit);

    Task DeleteAsync(Guid id);
}