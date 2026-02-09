namespace SparkTrack.AvaloniaImpl.Delegates;

using Controls.Attachment;
using Core.Shared.Data.Entities;

public delegate LocalAttachmentViewModel LocalAttachmentViewModelFactory(
    string path,
    Action<IAttachmentViewModel> onRemove
);

public delegate RemoteAttachmentViewModel RemoteAttachmentViewModelFactory(
    Attachment attachment,
    Action<IAttachmentViewModel> onRemove
);

public delegate ClipboardAttachmentViewModel ClipboardAttachmentViewModelFactory(
    string extension,
    byte[] data,
    Action<IAttachmentViewModel> onRemove
);