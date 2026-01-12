namespace SparkTrack.AvaloniaImpl.Pages.Feature;

using Controls.Attachment;
using System.Collections.ObjectModel;
using Controls.Comment;
using Controls.SubTask;
using Core.Client.Services.Users;
using Core.Shared.Data;
using Core.Shared.Data.Edit;
using Core.Shared.Data.Entities;
using Core.Shared.Enums;
using Core.Shared.Services.Features;
using DynamicData;
using Extensions;
using Fanatiki.MVVM.ViewModels;
using NLog;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Services.LocalFilesManager;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
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
    private readonly ILocalFilesManager                   m_localFilesManager;
    private readonly BehaviorSubject<IReadOnlyList<User>> m_availableEmployeesList = new([]);

    public FeaturePageViewModel(
        Guid projectId,
        Lazy<IScreen> hostScreen,
        IFeaturesService featuresService,
        IUsersService usersService,
        ILocalFilesManager localFilesManager
    ) : this(null, projectId, hostScreen, featuresService, usersService, localFilesManager) { }

    public FeaturePageViewModel(
        Feature feature,
        Lazy<IScreen> hostScreen,
        IFeaturesService featuresService,
        IUsersService usersService,
        ILocalFilesManager localFilesManager
    ) : this(feature, feature.Project.Id, hostScreen, featuresService, usersService, localFilesManager) { }

    private FeaturePageViewModel(
        Feature? feature,
        Guid projectId,
        Lazy<IScreen> hostScreen,
        IFeaturesService featuresService,
        IUsersService usersService,
        ILocalFilesManager localFilesManager
    )
    {
        m_feature = feature;
        m_projectId = projectId;
        m_hostScreen = hostScreen;
        m_featuresService = featuresService;
        m_usersService = usersService;
        m_localFilesManager = localFilesManager;

        InitializeProperties(feature);

        if (feature is null) IsNameEditing = true;

        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
        RefreshCommand = ReactiveCommand.CreateFromTask(RefreshAsync);
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

    public SuspendableObservableCollection<SubTaskViewModel> SubTasksList { get; } = [];

    public SuspendableObservableCollection<IAttachmentViewModel> AttachmentsList { get; } = [];

    public ObservableCollection<CommentViewModel> CommentsList { get; } =
    [
        new(
            new Comment
            {
                Author = new User
                {
                    Email = "max@asd",
                    Name = "Максимка Деченский",
                    Role = ERole.Employee,
                },
                Text = "Дело сделано",
                CreatedAt = DateTime.Now
            }
        ),
        new(
            new Comment
            {
                Author = new User
                {
                    Email = "kons@asd",
                    Name = "Костик Станиславский",
                    Role = ERole.Employee,
                },
                Text = "Это полный провал",
                CreatedAt = DateTime.Now.AddDays(-10),
                EditedAt = DateTime.Now
            }
        )
    ];

    [Reactive]
    public string Description { get; set; } = string.Empty;

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }

    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

    public async Task ChooseAttachmentsAsync()
    {
        var files = await m_localFilesManager.ChooseFilesForOpenAsync();

        foreach (var file in files.Where(it => !string.IsNullOrEmpty(it)))
            AddAttachment(file);
    }

    public void AddAttachment(string path)
    {
        var attachment = new LocalAttachmentViewModel(path, a => AttachmentsList.Remove(a));
        
        AttachmentsList.Add(attachment);
    }

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

        var availableEmployees = await m_usersService.GetPageAsync(ERole.Employee, PageQuery.All);
        m_availableEmployeesList.OnNext(availableEmployees.Items);

        if (m_feature?.Id is not { } featureId) return;

        m_feature = await m_featuresService.GetAsync(featureId);

        InitializeProperties(m_feature);
    }

    private async Task SaveAsync()
    {
        var editData = CreateEditData();

        if (m_feature is null)
        {
            await m_featuresService.AddAsync(editData);

            Back();
            return;
        }

        await m_featuresService.EditAsync(editData);
    }

    private FeatureEdit CreateEditData() => new()
    {
        Id = m_feature?.Id ?? -1,
        Name = Name,
        ProjectId = m_projectId,
        TasksList = SubTasksList.Select(it => it.MapToEdit()).ToArray(),
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
    }
}