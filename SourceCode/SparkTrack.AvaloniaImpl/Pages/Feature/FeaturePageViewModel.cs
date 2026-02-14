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
    private readonly SubTaskViewModelFactory              m_subTaskViewModelFactory;
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
        IAuthorizationService authorizationService,
        SubTaskViewModelFactory subTaskViewModelFactory
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
        authorizationService,
        subTaskViewModelFactory
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
        SubTaskViewModelFactory subTaskViewModelFactory
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
        authorizationService,
        subTaskViewModelFactory
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
        IAuthorizationService authorizationService,
        SubTaskViewModelFactory subTaskViewModelFactory
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
        m_subTaskViewModelFactory = subTaskViewModelFactory;
        IsDescriptionInPreviewMode = m_feature is not null;

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
    public string Description { get; set; } = string.Empty;

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }

    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

    public ReactiveCommand<Unit, Unit> RefreshCommentsCommand { get; }

    public ReactiveCommand<Unit, Unit> SaveCommentCommand { get; }

    public void OnImagePaste(byte[] image, string extension)
    {
        if(m_authorizationService.CurrentUser.Value?.Role is ERole.Employee) return;
        
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

        foreach (var taskViewModel in SubTasksList)
            taskViewModel.IsInEditMode = false;

        SubTasksList.Add(subTask);
    }

    private SubTaskViewModel CreateSubTaskViewModel(SubTask? subTask = null) => m_subTaskViewModelFactory.Invoke(
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
        Description = Description,
        Version = m_feature?.Version ?? Guid.Empty
    };

    private void InitializeProperties(Feature? feature)
    {
        Name = feature?.Name ?? "Название идеи";
        Description = feature?.Description ?? string.Empty;
        CreatedAt = feature?.CreatedAt;
        EditedAt = feature?.EditedAt;

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

        var commentsViewModels = page.Items.Select(it =>
        {
            var commentViewModel = m_commentFactory(it, OnCommentDeleteAsync);

            var editSubscription = commentViewModel.WhenAnyValue(vm => vm.EditViewModel)
                .WhereNotNull()
                .CombineLatest(Observable.Return(commentViewModel), (_, source) => source)
                .Subscribe(source =>
                    {
                        foreach (var otherComment in CommentsList)
                            if(otherComment != source) otherComment.CancelEdit();

                        CancelComment();
                    }
                );
            
            commentViewModel.DisposeWithViewModel(editSubscription);

            return commentViewModel;
        });

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

        await CommentEditViewModel.AttachmentsPanelViewModel.UploadLocalAttachments();

        var commentEdit = CommentEditViewModel.ToModel();

        await m_commentsService.AddAsync(m_feature.Id, commentEdit);

        CommentEditViewModel = null;

        await RefreshCommentsCommand.Execute().ToTask();
    }
}