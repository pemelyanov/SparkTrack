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
    RemoteAttachmentViewModelFactory remoteAttachmentViewModelFactory
)
    : ViewModelBase
{
    public SuspendableObservableCollection<IAttachmentViewModel> AttachmentsList { get; } = [];

    public Task UploadLocalAttachments()
    {
        var localAttachments =
            AttachmentsList.OfType<LocalAttachmentViewModel>().Where(it => it.UploadedFileId is null);

        var uploadingTasks = localAttachments.Select(it => it.UploadAsync());

        return Task.WhenAll(uploadingTasks);
    }

    public void ReplaceWithRemoteAttachments(IEnumerable<Attachment> attachmentsList)
    {
        var attachmentsViewModels =
            attachmentsList.Select(it => remoteAttachmentViewModelFactory(it, OnAttachmentDelete));

        using (AttachmentsList.SuspendNotifications())
        {
            AttachmentsList.Clear();
            AttachmentsList.AddRange(attachmentsViewModels);
        }
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

        AttachmentsList.Add(attachment);
    }

    private void OnAttachmentDelete(IAttachmentViewModel a) => AttachmentsList.Remove(a);
}