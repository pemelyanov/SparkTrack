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
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using Comment = Core.Shared.Data.Entities.Comment;
using SubTask = Core.Shared.Data.Entities.SubTask;

public class FeaturePageViewModel : ViewModelBase, IRoutableViewModel
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();

    private          Feature?                             m_feature;
    private readonly Guid                                 m_projectId;
    private readonly Lazy<IScreen>                        m_hostScreen;
    private readonly IFeaturesService                     m_featuresService;
    private readonly IUsersService                        m_usersService;
    private readonly Func<Comment?, CommentEditViewModel> m_commentEditFactory;
    private readonly ICommentsService                     m_commentsService;
    private readonly CommentViewModelFactory              m_commentFactory;
    private readonly IAuthorizationService                m_authorizationService;
    private readonly BehaviorSubject<IReadOnlyList<User>> m_availableEmployeesList = new([]);

    public FeaturePageViewModel(
        Guid projectId,
        Lazy<IScreen> hostScreen,
        IFeaturesService featuresService,
        IUsersService usersService,
        AttachmentsPanelViewModel attachmentsPanelViewModel,
        Func<Comment?, CommentEditViewModel> commentEditFactory,
        ICommentsService commentsService,
        CommentViewModelFactory commentFactory,
        IAuthorizationService authorizationService
    ) : this(
        null,
        projectId,
        hostScreen,
        featuresService,
        usersService,
        attachmentsPanelViewModel,
        commentEditFactory,
        commentsService,
        commentFactory,
        authorizationService
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
        IAuthorizationService authorizationService
    ) : this(
        feature,
        feature.Project.Id,
        hostScreen,
        featuresService,
        usersService,
        attachmentsPanelViewModel,
        commentEditFactory,
        commentsService,
        commentFactory,
        authorizationService
    ) { }

    private FeaturePageViewModel(
        Feature? feature,
        Guid projectId,
        Lazy<IScreen> hostScreen,
        IFeaturesService featuresService,
        IUsersService usersService,
        AttachmentsPanelViewModel attachmentsPanelViewModel,
        Func<Comment?, CommentEditViewModel> commentEditFactory,
        ICommentsService commentsService,
        CommentViewModelFactory commentFactory,
        IAuthorizationService authorizationService
    )
    {
        AttachmentsPanelViewModel = attachmentsPanelViewModel;
        m_feature = feature;
        m_projectId = projectId;
        m_hostScreen = hostScreen;
        m_featuresService = featuresService;
        m_usersService = usersService;
        m_commentEditFactory = commentEditFactory;
        m_commentsService = commentsService;
        m_commentFactory = commentFactory;
        m_authorizationService = authorizationService;
        IsDescriptionInPreviewMode = m_feature is not null;

        InitializeProperties(feature);

        if (feature is null) IsNameEditing = true;

        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);

        RefreshCommand = ReactiveCommand.CreateFromTask(
            () => Task.WhenAll([RefreshAsync(), RefreshCommentsAsync()])
        );

        RefreshCommentsCommand = ReactiveCommand.CreateFromTask(RefreshCommentsAsync);
        SaveCommentCommand = ReactiveCommand.CreateFromTask(SaveCommentAsync);
    }

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);

        RefreshCommand.Execute().Subscribe().DisposeWith(disposables);
    }

    public string UrlPathSegment => "feature";

    public IScreen HostScreen => m_hostScreen.Value;

    [Reactive]
    public string Name { get; set; } = string.Empty;

    [Reactive]
    public bool IsNameEditing { get; set; }

    [Reactive]
    public bool IsDescriptionInPreviewMode { get; set; }

    public bool CanAddComments => m_feature is not null;

    public AttachmentsPanelViewModel AttachmentsPanelViewModel { get; }

    [Reactive]
    public CommentEditViewModel? CommentEditViewModel { get; private set; }

    public SuspendableObservableCollection<SubTaskViewModel> SubTasksList { get; } = [];

    public SuspendableObservableCollection<CommentViewModel> CommentsList { get; } = [];

    [Reactive]
    public string Description { get; set; } = string.Empty;

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }

    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

    public ReactiveCommand<Unit, Unit> RefreshCommentsCommand { get; }

    public ReactiveCommand<Unit, Unit> SaveCommentCommand { get; }

    public void CreateComment() => CommentEditViewModel = m_commentEditFactory(null);

    public void CancelComment() => CommentEditViewModel = null;

    public void Back() => HostScreen.Router.BackOnUIThread();

    public void AddSubTask()
    {
        var subTask = CreateSubTaskViewModel();
        subTask.IsInEditMode = true;

        foreach (var taskViewModel in SubTasksList)
            taskViewModel.IsInEditMode = false;

        SubTasksList.Add(subTask);
    }

    private SubTaskViewModel CreateSubTaskViewModel(SubTask? subTask = null) => new(
        subTask,
        m_availableEmployeesList,
        it => SubTasksList.Remove(it)
    );

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

    private FeatureEdit CreateEditData() => new()
    {
        Id = m_feature?.Id ?? -1,
        Name = Name,
        ProjectId = m_projectId,
        TasksList = SubTasksList.Select(it => it.MapToEdit()).ToArray(),
        AttachmentsList = AttachmentsPanelViewModel.AttachmentsList.Select(it => it.ToModel()).ToArray(),
        Description = Description
    };

    private void InitializeProperties(Feature? feature)
    {
        Name = feature?.Name ?? "Название идеи";
        Description = feature?.Description ?? string.Empty;

        var subTasks = feature?.TasksList.Select(CreateSubTaskViewModel) ?? [];

        using (SubTasksList.SuspendNotifications())
        {
            SubTasksList.Clear();
            SubTasksList.AddRange(subTasks);
        }

        AttachmentsPanelViewModel.ReplaceWithRemoteAttachments(feature?.AttachmentsList ?? []);
    }

    private async Task RefreshCommentsAsync()
    {
        if (m_feature is null) return;

        var page = await m_commentsService.GetPageAsync(m_feature.Id, PageQuery.All);

        var commentsViewModels = page.Items.Select(it => m_commentFactory(it, OnCommentDeleteAsync));

        using (CommentsList.SuspendNotifications())
        {
            CommentsList.Clear();
            CommentsList.AddRange(commentsViewModels);
        }
    }

    private async Task OnCommentDeleteAsync(CommentViewModel comment)
    {
        // TODO: Добавить модалку подтверждения

        await m_commentsService.DeleteAsync(comment.Model.Id);

        CommentsList.Remove(comment);
    }

    private async Task SaveCommentAsync()
    {
        if (m_feature is null || CommentEditViewModel is null) return;

        await CommentEditViewModel.AttachmentsPanelViewModel.UploadLocalAttachments();

        var commentEdit = CommentEditViewModel.ToModel();

        await m_commentsService.AddAsync(m_feature.Id, commentEdit);

        CommentEditViewModel = null;

        await RefreshCommentsCommand.Execute().ToTask();
    }
}