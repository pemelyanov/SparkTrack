namespace SparkTrack.AvaloniaImpl.Controls.CommentEdit;

using AttachmentsPanel;
using Core.Shared.Data.Entities;

public class CommentEditViewModel(Comment? comment, AttachmentsPanelViewModel attachmentsPanelViewModel)
{
    public AttachmentsPanelViewModel AttachmentsPanelViewModel { get; } = attachmentsPanelViewModel;
}