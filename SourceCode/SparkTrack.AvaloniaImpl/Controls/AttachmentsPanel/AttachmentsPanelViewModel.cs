namespace SparkTrack.AvaloniaImpl.Controls.AttachmentsPanel;

using Attachment;
using Core.Shared.Data.Entities;
using Delegates;
using DynamicData;
using Fanatiki.MVVM.ViewModels;
using Services.LocalFilesManager;

public class AttachmentsPanelViewModel(
    ILocalFilesManager localFilesManager,
    LocalAttachmentViewModelFactory localLocalAttachmentViewModelFactory,
    RemoteAttachmentViewModelFactory remoteAttachmentViewModelFactory,
    ClipboardAttachmentViewModelFactory clipboardAttachmentViewModelFactory
)
    : ViewModelBase
{
    public event Action<IAttachmentViewModel>? AttachmentAdded;
    public event Action<IAttachmentViewModel>? PreviewAttachmentSetRequested;
    
    public SuspendableObservableCollection<IAttachmentViewModel> AttachmentsList { get; } = [];

    public Task UploadLocalAttachments()
    {
        var localAttachments =
            AttachmentsList.OfType<IUploadableAttachment>().Where(it => it.UploadedFileId is null);

        var uploadingTasks = localAttachments.Select(it => it.UploadAsync());

        return Task.WhenAll(uploadingTasks);
    }

    public void ReplaceWithRemoteAttachments(IEnumerable<Attachment> attachmentsList)
    {
        var attachmentsViewModels =
            attachmentsList.Select(it =>
            {
                var viewModel = remoteAttachmentViewModelFactory(it, OnAttachmentDelete);
                viewModel.PreviewSetRequested += Attachment_OnPreviewSetRequested;

                return viewModel;
            });

        var previousAttachments = AttachmentsList.ToArray();
        using (AttachmentsList.SuspendNotifications())
        {
            AttachmentsList.Clear();
            AttachmentsList.AddRange(attachmentsViewModels);
        }

        foreach (var attachmentViewModel in previousAttachments)
            attachmentViewModel.PreviewSetRequested -= Attachment_OnPreviewSetRequested;
    }

    public async Task ChooseAttachmentsAsync()
    {
        var files = await localFilesManager.ChooseFilesForOpenAsync();

        foreach (var file in files.Where(it => !string.IsNullOrEmpty(it)))
            AddAttachment(file);
    }

    public void AddAttachment(string path)
    {
        var attachment = localLocalAttachmentViewModelFactory.Invoke(path, OnAttachmentDelete);
        attachment.PreviewSetRequested += Attachment_OnPreviewSetRequested;

        AttachmentsList.Add(attachment);
        AttachmentAdded?.Invoke(attachment);
    }

    public void AddAttachment(byte[] data, string extension)
    {
        var attachment = clipboardAttachmentViewModelFactory.Invoke(extension, data, OnAttachmentDelete);
        attachment.PreviewSetRequested += Attachment_OnPreviewSetRequested;

        AttachmentsList.Add(attachment);
        AttachmentAdded?.Invoke(attachment);
    }

    private void OnAttachmentDelete(IAttachmentViewModel a)
    {
        AttachmentsList.Remove(a);
        a.PreviewSetRequested -= Attachment_OnPreviewSetRequested;
    }

    private void Attachment_OnPreviewSetRequested(IAttachmentViewModel preview)
    { 
        PreviewAttachmentSetRequested?.Invoke(preview);
    }
}