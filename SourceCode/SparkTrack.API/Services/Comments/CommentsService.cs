namespace SparkTrack.API.Services.Comments;

using Core.Shared.Data;
using Core.Shared.Data.Edit;
using Core.Shared.Data.Entities;
using Core.Shared.Services.Comments;
using Delegates;
using MappingExtensions;

public class CommentsService(ClientFactory<FeaturesClient> featuresClientFactory) : ICommentsService
{
    public async Task<IReadOnlyPagedData<Comment>> GetPageAsync(int featureId, PageQuery pageQuery)
    {
        using var wrapper = featuresClientFactory();
        var page = await wrapper.Client.GetCommentsPageAsync(featureId, pageQuery.Page, pageQuery.ItemsPerPage);

        var items = page.Items.Select(it => it.ToDomain()).ToArray();

        return new ReadOnlyPagedData<Comment>(items, page.Total);
    }

    public async Task AddAsync(int featureId, CommentEdit commentEdit)
    {
        using var wrapper = featuresClientFactory();

        await wrapper.Client.AddCommentAsync(featureId, commentEdit.ToDTO());
    }

    public async Task<Comment?> EditAsync(CommentEdit commentEdit)
    {
        using var wrapper = featuresClientFactory();

        var dto = await wrapper.Client.EditCommentAsync(commentEdit.ToDTO());

        return dto.ToDomain();
    }

    public async Task DeleteAsync(Guid id)
    {
        using var wrapper = featuresClientFactory();

        await wrapper.Client.DeleteCommentAsync(id);
    }
}