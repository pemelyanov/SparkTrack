namespace SparkTrack.DataAccess.EFCore.Repositories;

using Core.Repositories;
using Core.Shared.Data;
using Core.Shared.Data.Entities;
using Data.Entities;
using Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public class CommentsRepository(SparkTrackDbContext dbContext) : ICommentsRepository
{
    public Task<IReadOnlyPagedData<Comment>> GetPageAsync(int featureId, PageQuery pageQuery) => dbContext.Comments
        .AsNoTracking()
        .Where(it => it.FeatureId == featureId)
        .OrderBy(it => it.CreatedAt)
        .Select(
            GetCommentMappingExpression()
        )
        .AsPaginated(pageQuery)
        .CollectAsync();

    public async Task AddAsync(int featureId, Comment comment)
    {
        var commentData = new CommentData
        {
            Text = comment.Text,
            FeatureId = featureId,
            UserId = comment.Author.Id,
            CreatedAt = comment.CreatedAt,
            AttachmentsList = comment.AttachmentsList.Select(AttachmentsUtils.ToAttachmentData).ToArray()
        };

        await dbContext.Comments.AddAsync(commentData);
        await dbContext.SaveChangesAsync();
    }

    public async Task<Guid?> GetAuthorIdAsync(Guid commentId)
    {
        var comment = await dbContext.Comments.FindAsync(commentId);

        return comment?.UserId;
    }

    public async Task<Comment?> EditAsync(Comment comment)
    {
        var existingComment = await dbContext.Comments
            .Where(it => it.Id == comment.Id)
            .Include(it => it.User)
            .Include(it => it.AttachmentsList)
            .FirstOrDefaultAsync();

        if (existingComment is null) return null;

        existingComment.Text = comment.Text;
        existingComment.EditedAt = comment.EditedAt;

        AttachmentsUtils.HandleAttachments(dbContext, comment.AttachmentsList, existingComment);

        await dbContext.SaveChangesAsync();

        return GetCommentMappingExpression().Compile().Invoke(existingComment);
    }

    public async Task DeleteAsync(Guid id)
    {
        var comment = await dbContext.Comments.FindAsync(id);

        if (comment is null) return;

        dbContext.Comments.Remove(comment);
        await dbContext.SaveChangesAsync();
    }

    private static Expression<Func<CommentData, Comment>> GetCommentMappingExpression()
    {
        return it => new Comment
        {
            Id = it.Id,
            Author = new User
            {
                Id = it.UserId,
                Email = it.User.Email,
                Name = it.User.Name,
                Role = it.User.Role,
                ArchivedAt = it.User.ArchivedAt,
                ArchiveSource = it.User.ArchiveSource
            },
            Text = it.Text,
            CreatedAt = it.CreatedAt,
            EditedAt = it.EditedAt,
            AttachmentsList = it.AttachmentsList.Select(
                    a => new Attachment
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Extension = a.Extension,
                        Checksum = a.Checksum,
                        Size = a.Size,
                        FileId = a.FileId,
                        IsImage = a.IsImage
                    }
                )
                .ToArray()
        };
    }
}