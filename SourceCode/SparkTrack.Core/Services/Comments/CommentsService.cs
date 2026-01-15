namespace SparkTrack.Core.Services.Comments;

using Authorization;
using Exceptions;
using Extensions;
using Repositories;
using Shared.Data;
using Shared.Data.Edit;
using Shared.Data.Entities;
using Shared.Services.Comments;

public class CommentsService(ICommentsRepository commentsRepository, IAuthorizationService authorizationService)
    : ICommentsService
{
    public Task<IReadOnlyPagedData<Comment>> GetPageAsync(int featureId, PageQuery pageQuery) =>
        commentsRepository.GetPageAsync(featureId, pageQuery);

    public Task AddAsync(int featureId, CommentEdit commentEdit) =>
        commentsRepository.AddAsync(featureId, ToComment(commentEdit, createdAt: DateTime.UtcNow));

    public async Task<Comment?> EditAsync(CommentEdit commentEdit)
    {
        var authorId = await commentsRepository.GetAuthorIdAsync(commentEdit.Id);

        if (authorId is null) return null;

        if (authorId != authorizationService.CurrentUser?.Id) throw new ForbiddenException();

        return await commentsRepository.EditAsync(ToComment(commentEdit, editedAt: DateTime.UtcNow));
    }

    public async Task DeleteAsync(Guid id)
    {
        var authorId = await commentsRepository.GetAuthorIdAsync(id);

        if (authorId is null) return;

        if (authorId != authorizationService.CurrentUser?.Id) throw new ForbiddenException();
        
        await commentsRepository.DeleteAsync(id);
    }

    private Comment ToComment(CommentEdit commentEdit, DateTime? createdAt = null, DateTime? editedAt = null) => new()
    {
        Id = commentEdit.Id,
        Text = commentEdit.Text,
        Author = authorizationService.GetUserOrThrowIfUnauthorized(),
        AttachmentsList = commentEdit.AttachmentsList,
        CreatedAt = createdAt ?? default,
        EditedAt = editedAt
    };
}