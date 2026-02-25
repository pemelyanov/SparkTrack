using SparkTrack.AvaloniaImpl.Controls.TemplateSelectionForm;
using SparkTrack.AvaloniaImpl.Data.Templates;
using SparkTrack.AvaloniaImpl.Services.DialogHost;

namespace SparkTrack.AvaloniaImpl.Pages.Feature;

using Controls.AttachmentsPanel;
using Controls.Comment;
using Controls.CommentEdit;
using Controls.SubTask;
using Core.Client.Services.Authorization;
using Core.Client.Services.Users;
using Core.Shared.Data;
using Core.Shared.Data.Edit;
using Core.Shared.Data.Entities;
using Core.Shared.Enums;
using Core.Shared.Services.Comments;
using Core.Shared.Services.Features;
using Delegates;
using DynamicData;
using Extensions;
using Fanatiki.MVVM.ViewModels;
using NLog;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text.Json;
using Controls.Attachment;
using Controls.TemplateSaveForm;
using Core.Client.Enums;
using Core.Client.Services.PopupNotification;
using DescriptionTemplates;
using Exceptions;
using Comment = Core.Shared.Data.Entities.Comment;
using SubTask = Core.Shared.Data.Entities.SubTask;

public class FeaturePageViewModel : ViewModelBase, IRoutableViewModel
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();

    private          Feature? m_feature;
    private readonly Project m_project;
    private readonly Lazy<IScreen> m_hostScreen;
    private readonly IFeaturesService m_featuresService;
    private readonly IUsersService m_usersService;
    private readonly Func<Comment?, CommentEditViewModel> m_commentEditFactory;
    private readonly ICommentsService m_commentsService;
    private readonly CommentViewModelFactory m_commentFactory;
    private readonly IAuthorizationService m_authorizationService;
    private readonly SubTaskViewModelFactory m_subTaskViewModelFactory;
    private readonly IPopupNotificationService m_popupNotificationService;
    private readonly Func<TemplateSelectionFormViewModel<SubTaskTemplate>> m_subTaskTemplateSelectionViewModelFactory;
    private readonly IDialogService m_dialogService;
    private readonly Func<FeatureTemplate, TemplateSaveFormViewModel<FeatureTemplate>> m_templateViewModelFactory;
    private readonly BehaviorSubject<IReadOnlyList<User>> m_availableEmployeesList = new([]);

    public FeaturePageViewModel(
        Project project,
        Lazy<IScreen> hostScreen,
        IFeaturesService featuresService,
        IUsersService usersService,
        AttachmentsPanelViewModel attachmentsPanelViewModel,
        Func<Comment?, CommentEditViewModel> commentEditFactory,
        ICommentsService commentsService,
        CommentViewModelFactory commentFactory,
        IAuthorizationService authorizationService,
        SubTaskViewModelFactory subTaskViewModelFactory,
        IPopupNotificationService popupNotificationService,
        Func<TemplateSelectionFormViewModel<SubTaskTemplate>> subTaskTemplateSelectionViewModelFactory,
        IDialogService dialogService,
        Func<FeatureTemplate, TemplateSaveFormViewModel<FeatureTemplate>> templateViewModelFactory
    ) : this(
        null,
        project,
        hostScreen,
        featuresService,
        usersService,
        attachmentsPanelViewModel,
        commentEditFactory,
        commentsService,
        commentFactory,
        authorizationService,
        subTaskViewModelFactory,
        popupNotificationService,
        subTaskTemplateSelectionViewModelFactory,
        dialogService,
        templateViewModelFactory
    ) { }

    public FeaturePageViewModel(
        Feature feature,
        Lazy<IScreen> hostScreen,
        IFeaturesService featuresService,
        IUsersService usersService,
        AttachmentsPanelViewModel attachmentsPanelViewModel,
        Func<Comment?, CommentEditViewModel> commentEditFactory,
        ICommentsService commentsService,
        CommentViewModelFactory commentFactory,
        IAuthorizationService authorizationService,
        SubTaskViewModelFactory subTaskViewModelFactory,
        IPopupNotificationService popupNotificationService,
        Func<TemplateSelectionFormViewModel<SubTaskTemplate>> subTaskTemplateSelectionViewModelFactory,
        IDialogService dialogService,
        Func<FeatureTemplate, TemplateSaveFormViewModel<FeatureTemplate>> templateViewModelFactory
    ) : this(
        feature,
        feature.Project,
        hostScreen,
        featuresService,
        usersService,
        attachmentsPanelViewModel,
        commentEditFactory,
        commentsService,
        commentFactory,
        authorizationService,
        subTaskViewModelFactory,
        popupNotificationService,
        subTaskTemplateSelectionViewModelFactory,
        dialogService,
        templateViewModelFactory
    ) { }

    private FeaturePageViewModel(
        Feature? feature,
        Project project,
        Lazy<IScreen> hostScreen,
        IFeaturesService featuresService,
        IUsersService usersService,
        AttachmentsPanelViewModel attachmentsPanelViewModel,
        Func<Comment?, CommentEditViewModel> commentEditFactory,
        ICommentsService commentsService,
        CommentViewModelFactory commentFactory,
        IAuthorizationService authorizationService,
        SubTaskViewModelFactory subTaskViewModelFactory,
        IPopupNotificationService popupNotificationService,
        Func<TemplateSelectionFormViewModel<SubTaskTemplate>> subTaskTemplateSelectionViewModelFactory,
        IDialogService dialogService,
        Func<FeatureTemplate, TemplateSaveFormViewModel<FeatureTemplate>> templateViewModelFactory
    )
    {
        AttachmentsPanelViewModel = attachmentsPanelViewModel;
        m_feature = feature;
        m_project = project;
        m_hostScreen = hostScreen;
        m_featuresService = featuresService;
        m_usersService = usersService;
        m_commentEditFactory = commentEditFactory;
        m_commentsService = commentsService;
        m_commentFactory = commentFactory;
        m_authorizationService = authorizationService;
        m_subTaskViewModelFactory = subTaskViewModelFactory;
        m_popupNotificationService = popupNotificationService;
        m_subTaskTemplateSelectionViewModelFactory = subTaskTemplateSelectionViewModelFactory;
        m_dialogService = dialogService;
        m_templateViewModelFactory = templateViewModelFactory;
        IsReelDescriptionInPreviewMode = IsPreviewDescriptionInPreviewMode = m_feature is not null;
        IsEditingLink = m_feature is null;
        AttachmentsPanelViewModel.AttachmentAdded += AttachmentsPanelViewModel_OnAttachmentAdded;
        AttachmentsPanelViewModel.PreviewAttachmentSetRequested +=
            AttachmentsPanelViewModel_OnPreviewAttachmentSetRequested;

        if (feature is null) IsNameEditing = true;

        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);

        RefreshCommand = ReactiveCommand.CreateFromTask(() => 
            Task.WhenAll(RefreshAsync(), RefreshCommentsAsync()));

        RefreshCommentsCommand = ReactiveCommand.CreateFromTask(RefreshCommentsAsync);
        SaveCommentCommand = ReactiveCommand.CreateFromTask(SaveCommentAsync);
    }

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);

        RefreshCommand.Execute().Subscribe().DisposeWith(disposables);

        this.WhenAnyValue(
                it => it.IsEditingSubTask,
                it => it.IsEditingComment,
                (isEditingSubTask, isEditingComment) => !isEditingSubTask && !isEditingComment
            )
            .Subscribe(canSaveByHotKey => CanSaveByHotKey = canSaveByHotKey)
            .DisposeWith(disposables);
    }

    public string UrlPathSegment => "feature";

    public IScreen HostScreen => m_hostScreen.Value;

    [Reactive]
    public string Name { get; set; } = string.Empty;

    [Reactive]
    public IAttachmentViewModel? PreviewAttachment { get; private set; }

    [Reactive]
    public bool IsNameEditing { get; set; }

    [Reactive]
    public bool IsReelDescriptionInPreviewMode { get; set; }

    [Reactive]
    public bool IsPreviewDescriptionInPreviewMode { get; set; }

    [Reactive]
    public bool IsEditingLink { get; set; }

    [Reactive]
    public bool IsEditingComment { get; private set; }

    [Reactive]
    public bool IsEditingSubTask { get; private set; }

    [Reactive]
    public bool CanSaveByHotKey { get; private set; }

    public bool CanAddComments => m_feature is not null;

    [Reactive]
    public DateTime? CreatedAt { get; private set; }

    [Reactive]
    public DateTime? EditedAt { get; private set; }

    public AttachmentsPanelViewModel AttachmentsPanelViewModel { get; }

    [Reactive]
    public CommentEditViewModel? CommentEditViewModel { get; private set; }

    public SuspendableObservableCollection<SubTaskViewModel> SubTasksList { get; } = [];

    public SuspendableObservableCollection<CommentViewModel> CommentsList { get; } = [];

    [Reactive]
    public string ReelLink { get; set; } = string.Empty;

    [Reactive]
    public string ReelDescription { get; set; } = string.Empty;

    [Reactive]
    public string PreviewDescription { get; set; } = string.Empty;

    public Project Project => m_project;

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }

    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

    public ReactiveCommand<Unit, Unit> RefreshCommentsCommand { get; }

    public ReactiveCommand<Unit, Unit> SaveCommentCommand { get; }

    public async Task CreateTemplateAsync()
    {
        var template = new FeatureTemplate
        {
            Name = Name,
            Description = GetDescription(),
            TasksList = SubTasksList.Select(it => it.GetTemplate()).ToArray(),
        };

        var viewModel = m_templateViewModelFactory(template);

        await m_dialogService.ShowAsync(viewModel);
    }

    public void OnImagePaste(byte[] image, string extension)
    {
        if (m_authorizationService.CurrentUser.Value?.Role is ERole.Employee) return;

        AttachmentsPanelViewModel.AddAttachment(image, extension);
    }

    public void CreateComment()
    {
        CommentEditViewModel = m_commentEditFactory(null);

        foreach (var commentViewModel in CommentsList)
            commentViewModel.CancelEdit();
    }

    public void CancelComment() => CommentEditViewModel = null;

    public void Back() => HostScreen.Router.BackOnUIThread();

    public void AddSubTask()
    {
        var subTask = CreateSubTaskViewModel();
        subTask.IsInEditMode = true;

        SubTasksList.Add(subTask);
    }

    public async Task AddSubTaskFromTemplateAsync()
    {
        var selectionViewModel = m_subTaskTemplateSelectionViewModelFactory();

        if (await m_dialogService.ShowAsync(selectionViewModel) is not true ||
            selectionViewModel.SelectedTemplate is not SubTaskTemplate template) return;

        SubTaskViewModel subTask = CreateSubTaskFromTemplate(template);
        subTask.IsInEditMode = true;

        SubTasksList.Add(subTask);
    }

    public void InitializeFromTemplate(FeatureTemplate template)
    {
        Name = template.Name;

        InitializeDescriptionProperties(template.Description);

        var subTasks = template.TasksList.Select(CreateSubTaskFromTemplate);

        SubTasksList.AddRange(subTasks);
    }

    private SubTaskViewModel CreateSubTaskFromTemplate(SubTaskTemplate template)
    {
        var subTask = CreateSubTaskViewModel();
        subTask.Name = template.Name;
        subTask.EmployeeToSelectOnNextLoad = template.ExecutorEmployee;

        subTask.Cost = template.Cost;
        subTask.TimelyBonus = template.TimelyBonus;
        subTask.Deadline = DateTime.Now + template.Deadline;
        return subTask;
    }

    private SubTaskViewModel CreateSubTaskViewModel(SubTask? subTask = null)
    {
        var subTaskViewModel = m_subTaskViewModelFactory.Invoke(
            subTask,
            m_availableEmployeesList,
            it => SubTasksList.Remove(it)
        );

        var isEditSubscription = subTaskViewModel.WhenAnyValue(it => it.IsInEditMode)
            .Do(_ => IsEditingSubTask = SubTasksList.Any(it => it.IsInEditMode))
            .Where(isInEditMode => isInEditMode)
            .Subscribe(_ =>
                {
                    foreach (var task in SubTasksList)
                        if (subTaskViewModel != task)
                            task.IsInEditMode = false;
                }
            );

        subTaskViewModel.DisposeWithViewModel(isEditSubscription);

        return subTaskViewModel;
    }

    private async Task RefreshAsync()
    {
        s_logger.Info("Refresh executed");

        if (m_authorizationService.CurrentUser.Value?.Role is not ERole.Employee)
        {
            var availableEmployees = await m_usersService.GetPageAsync(ERole.Employee, PageQuery.All);
            m_availableEmployeesList.OnNext(availableEmployees.Items);
        }

        if (m_feature?.Id is not { } featureId) return;

        m_feature = await m_featuresService.GetAsync(featureId);

        InitializeProperties(m_feature);
    }

    private async Task SaveAsync()
    {
        try
        {
            s_logger.Info("Saving feature {id}", m_feature?.Id);
            
            await AttachmentsPanelViewModel.UploadLocalAttachments();

            var editData = CreateEditData();

            if (m_feature is null)
            {
                await m_featuresService.AddAsync(editData);

                Back();

                return;
            }

            await m_featuresService.EditAsync(editData);

            await RefreshCommand.Execute().ToTask();
        }
        catch (Exception e)
        {
            if (e is NotifyUIException)
                s_logger.Warn(e.Message);
            else
                s_logger.Error(e);

            m_popupNotificationService.Show(ENotificationType.Error, e.Message, "Ошибка сохранения");
        }
    }

    private FeatureEdit CreateEditData() => new()
    {
        Id = m_feature?.Id ?? -1,
        Name = Name,
        ProjectId = m_project.Id,
        TasksList = SubTasksList.Select(it => it.MapToEdit()).ToArray(),
        AttachmentsList = AttachmentsPanelViewModel.AttachmentsList.Select(it => it.ToModel()).ToArray(),
        Description = GetDescription(),
        Version = m_feature?.Version ?? Guid.Empty
    };

    private string GetDescription() => JsonSerializer.Serialize(
        new ReelWithPreviewTemplate
        {
            ReelLink = ReelLink,
            PreviewDescription = PreviewDescription,
            ReelDescription = ReelDescription,
            PreviewAttachmentName = PreviewAttachment?.Name
        }
    );

    private void InitializeProperties(Feature? feature)
    {
        Name = feature?.Name ?? "Название идеи";

        AttachmentsPanelViewModel.ReplaceWithRemoteAttachments(feature?.AttachmentsList ?? []);
        InitializeDescriptionProperties(feature?.Description);
        CreatedAt = feature?.CreatedAt;
        EditedAt = feature?.EditedAt;

        var subTasks = feature?.TasksList.Select(CreateSubTaskViewModel) ?? [];

        var oldTasks = SubTasksList.ToArray();
        using (SubTasksList.SuspendNotifications())
        {
            SubTasksList.Clear();
            SubTasksList.AddRange(subTasks);
        }

        foreach (var oldTask in oldTasks)
            oldTask.Dispose();
    }

    private void InitializeDescriptionProperties(string? description)
    {
        if (string.IsNullOrEmpty(description)) return;

        var template = TryParseJson<ReelWithPreviewTemplate>(description);

        ReelLink = template?.ReelLink ?? string.Empty;
        ReelDescription = template?.ReelDescription ?? string.Empty;
        PreviewDescription = template?.PreviewDescription ?? string.Empty;

        PreviewAttachment =
            AttachmentsPanelViewModel.AttachmentsList.FirstOrDefault(it => it.Name == template?.PreviewAttachmentName);
    }

    private TData? TryParseJson<TData>(string? data) where TData : class
    {
        if (data is null) return null;

        try
        {
            return JsonSerializer.Deserialize<TData>(data);
        }
        catch
        {
            return null;
        }
    }

    private async Task RefreshCommentsAsync()
    {
        if (m_feature is null) return;

        var page = await m_commentsService.GetPageAsync(m_feature.Id, PageQuery.All);

        var commentsViewModels = page.Items.Select(it =>
            {
                var commentViewModel = m_commentFactory(it, OnCommentDeleteAsync);

                var editSubscription = commentViewModel.WhenAnyValue(vm => vm.EditViewModel)
                    .Do(_ => IsEditingComment = CommentsList.Any(vm => vm.EditViewModel is not null))
                    .WhereNotNull()
                    .CombineLatest(Observable.Return(commentViewModel), (_, source) => source)
                    .Subscribe(source =>
                        {
                            foreach (var otherComment in CommentsList)
                                if (otherComment != source)
                                    otherComment.CancelEdit();

                            CancelComment();
                        }
                    );

                commentViewModel.DisposeWithViewModel(editSubscription);

                return commentViewModel;
            }
        );

        var oldComments = CommentsList.ToArray();

        using (CommentsList.SuspendNotifications())
        {
            CommentsList.Clear();
            CommentsList.AddRange(commentsViewModels);
        }

        foreach (var commentViewModel in oldComments)
            commentViewModel.Dispose();
    }

    private async Task OnCommentDeleteAsync(CommentViewModel comment)
    {
        await m_commentsService.DeleteAsync(comment.Model.Id);

        CommentsList.Remove(comment);
    }

    private async Task SaveCommentAsync()
    {
        if (m_feature is null || CommentEditViewModel is null) return;

        try
        {
            s_logger.Info("Creating comment...");
            
            await CommentEditViewModel.AttachmentsPanelViewModel.UploadLocalAttachments();

            var commentEdit = CommentEditViewModel.ToModel();

            await m_commentsService.AddAsync(m_feature.Id, commentEdit);

            CommentEditViewModel = null;

            await RefreshCommentsCommand.Execute().ToTask();
            
            s_logger.Info("Comment created");
        }
        catch (Exception e)
        {
            if (e is NotifyUIException)
                s_logger.Warn(e.Message);
            else
                s_logger.Error(e);

            m_popupNotificationService.Show(ENotificationType.Error, e.Message, "Ошибка создания комментария");
        }
       
    }

    private void AttachmentsPanelViewModel_OnPreviewAttachmentSetRequested(IAttachmentViewModel preview)
    {
        PreviewAttachment = preview;
    }

    private void AttachmentsPanelViewModel_OnAttachmentAdded(IAttachmentViewModel attachment)
    {
        if (AttachmentsPanelViewModel.AttachmentsList.Count == 1 && attachment.IsImage) PreviewAttachment = attachment;
    }
}