namespace SparkTrack.Core.Services.Comments;

using Authorization;
using Exceptions;
using Repositories;
using Shared.Data;
using Shared.Data.Edit;
using Shared.Data.Entities;

public class CommentsService(ICommentsRepository commentsRepository, IAuthorizationService authorizationService)
    : ICommentsService
{
    public Task<IReadOnlyPagedData<Comment>> GetPageAsync(Guid featureId, PageQuery pageQuery) =>
        commentsRepository.GetPageAsync(featureId, pageQuery);

    public Task AddAsync(Comment comment) => commentsRepository.AddAsync(comment);

    public async Task<Comment?> EditAsync(CommentEdit commentEdit)
    {
        var authorId = await commentsRepository.GetAuthorIdAsync(commentEdit.Id);

        if (authorId is null) return null;

        if (authorId != authorizationService.CurrentUser?.Id) throw new ForbiddenException();

        return await commentsRepository.EditAsync(commentEdit);
    }

    public Task DeleteAsync(Guid id) => commentsRepository.DeleteAsync(id);
}