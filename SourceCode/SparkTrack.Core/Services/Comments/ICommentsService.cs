namespace SparkTrack.Core.Services.Comments;

using Shared.Data;
using Shared.Data.Edit;
using Shared.Data.Entities;

public interface ICommentsService
{
    Task<IReadOnlyPagedData<Comment>> GetPageAsync(int featureId, PageQuery pageQuery);

    Task AddAsync(int featureId, CommentEdit commentEdit);
    
    Task<Comment?> EditAsync(CommentEdit commentEdit);

    Task DeleteAsync(Guid id);
}