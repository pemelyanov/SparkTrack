namespace SparkTrack.Core.Services.Comments;

using Shared.Data;
using Shared.Data.Edit;
using Shared.Data.Entities;

public interface ICommentsService
{
    Task<IReadOnlyPagedData<Comment>> GetPageAsync(Guid featureId, PageQuery pageQuery);

    Task AddAsync(Comment comment);
    
    Task<Comment?> EditAsync(CommentEdit commentEdit);

    Task DeleteAsync(Guid id);
}