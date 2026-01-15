namespace SparkTrack.AvaloniaImpl.Controls.CommentEdit;

using AttachmentsPanel;

public class CommentEditViewModel(AttachmentsPanelViewModel attachmentsPanelViewModel)
{
    public AttachmentsPanelViewModel AttachmentsPanelViewModel { get; } = attachmentsPanelViewModel;
}