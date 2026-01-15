namespace SparkTrack.AvaloniaImpl.Controls.CommentEdit;

using AttachmentsPanel;
using Core.Shared.Data.Entities;
using ReactiveUI.Fody.Helpers;

public class CommentEditViewModel
{
    private readonly Comment? m_comment;

    public CommentEditViewModel(Comment? comment, AttachmentsPanelViewModel attachmentsPanelViewModel)
    {
        m_comment = comment;
        Text = comment?.Text ?? string.Empty;
        AttachmentsPanelViewModel = attachmentsPanelViewModel;

        if (comment is not null)
            AttachmentsPanelViewModel.ReplaceWithRemoteAttachments(comment.AttachmentsList);
    }

    [Reactive]
    public string Text { get; set; }

    public AttachmentsPanelViewModel AttachmentsPanelViewModel { get; }

    public Core.Shared.Data.Edit.CommentEdit ToModel() => new()
    {
        Id = m_comment?.Id ?? Guid.Empty,
        Text = Text,
        AttachmentsList = AttachmentsPanelViewModel.AttachmentsList.Select(it => it.ToModel()).ToArray()
    };
}