namespace SparkTrack.Core.Shared.Services.Comments;

using Data;
using Data.Edit;
using Data.Entities;

public interface ICommentsService
{
    Task<IReadOnlyPagedData<Comment>> GetPageAsync(int featureId, PageQuery pageQuery);

    Task AddAsync(int featureId, CommentEdit commentEdit);
    
    Task<Comment?> EditAsync(CommentEdit commentEdit);

    Task DeleteAsync(Guid id);
}