namespace SparkTrack.Core.Services.Comments;

using Shared.Data;
using Shared.Data.Edit;
using Shared.Data.Entities;

public interface ICommentsService
{
    Task<IReadOnlyPagedData<Comment>> GetPageAsync(Guid featureId, PageQuery pageQuery);

    Task AddAsync(Guid featureId, CommentEdit commentEdit);
    
    Task<Comment?> EditAsync(CommentEdit commentEdit);

    Task DeleteAsync(Guid id);
}