namespace SparkTrack.AvaloniaImpl.Controls.Comment;

using CommentEdit;
using Core.Shared.Services.Comments;
using Extensions;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Services.DialogHost;
using System.Reactive;
using Core.Client.Enums;
using Core.Client.Services.PopupNotification;
using Exceptions;
using NLog;
using CommentModel = Core.Shared.Data.Entities.Comment;

public class CommentViewModel : ViewModelBase
{
    private static readonly ILogger                                   s_logger = LogManager.GetCurrentClassLogger();
    private readonly        Func<CommentModel?, CommentEditViewModel> m_editViewModelFactory;
    private readonly        ICommentsService                          m_commentsService;
    private readonly        IPopupNotificationService                 m_popupNotificationService;

    public CommentViewModel(
        CommentModel model,
        Func<CommentViewModel, Task> onDelete,
        Func<CommentModel?, CommentEditViewModel> editViewModelFactory,
        ICommentsService commentsService,
        IDialogService dialogService,
        IPopupNotificationService popupNotificationService
    )
    {
        m_editViewModelFactory = editViewModelFactory;
        m_commentsService = commentsService;
        m_popupNotificationService = popupNotificationService;
        Model = model;

        DeleteCommand = ReactiveCommand.CreateFromTask(
            async () =>
            {
                if (!await dialogService.ConfirmAsync(
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

        try
        {
            s_logger.Info("Saving comment {id}...", Model.Id);
            await EditViewModel.AttachmentsPanelViewModel.UploadLocalAttachments();

            var commentEdit = EditViewModel.ToModel();

            var newModel = await m_commentsService.EditAsync(commentEdit);

            if (newModel != null)
                Model = newModel;
            s_logger.Info("Comment {id} saved", Model.Id);

            EditViewModel = null;
        }
        catch (Exception e)
        {
            if (e is NotifyUIException)
                s_logger.Warn(e.Message);
            else
                s_logger.Error(e);

            m_popupNotificationService.Show(ENotificationType.Error, e.Message, "Ошибка сохранения комментария");
        }
       
    }
}