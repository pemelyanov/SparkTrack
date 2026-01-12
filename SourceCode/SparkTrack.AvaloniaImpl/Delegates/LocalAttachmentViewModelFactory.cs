namespace SparkTrack.AvaloniaImpl.Delegates;

using Controls.Attachment;

public delegate LocalAttachmentViewModel LocalAttachmentViewModelFactory(string path, Action<IAttachmentViewModel> onRemove);