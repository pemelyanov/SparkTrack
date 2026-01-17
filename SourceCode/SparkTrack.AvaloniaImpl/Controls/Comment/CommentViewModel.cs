namespace SparkTrack.AvaloniaImpl.Controls.Comment;

using CommentEdit;
using Core.Shared.Services.Comments;
using Extensions;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Services.DialogHost;
using System.Reactive;
using CommentModel = Core.Shared.Data.Entities.Comment;

public class CommentViewModel : ViewModelBase
{
    private readonly Func<CommentModel?, CommentEditViewModel> m_editViewModelFactory;
    private readonly ICommentsService                          m_commentsService;

    public CommentViewModel(
        CommentModel model,
        Func<CommentViewModel, Task> onDelete,
        Func<CommentModel?, CommentEditViewModel> editViewModelFactory,
        ICommentsService commentsService,
        IDialogHost dialogHost
    )
    {
        m_editViewModelFactory = editViewModelFactory;
        m_commentsService = commentsService;
        Model = model;

        DeleteCommand = ReactiveCommand.CreateFromTask(
            async () =>
            {
                if (!await dialogHost.ConfirmAsync(
                    "Вы уверены что хотите удалить комментарий?",
                    "Удаление комментария"
                )) return;

                await onDelete(this);
            }
        );
        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
    }

    [Reactive]
    public CommentModel Model { get; private set; }

    [Reactive]
    public CommentEditViewModel? EditViewModel { get; private set; }

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }

    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }

    public void Edit() => EditViewModel = m_editViewModelFactory.Invoke(Model);

    public void CancelEdit() => EditViewModel = null;

    private async Task SaveAsync()
    {
        if (EditViewModel is null) return;

        await EditViewModel.AttachmentsPanelViewModel.UploadLocalAttachments();

        var commentEdit = EditViewModel.ToModel();

        var newModel = await m_commentsService.EditAsync(commentEdit);

        if (newModel != null)
            Model = newModel;

        EditViewModel = null;
    }
}