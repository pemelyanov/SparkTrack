namespace SparkTrack.AvaloniaImpl.Pages.Feature;

using System.Collections.ObjectModel;
using Controls.Comment;
using Controls.SubTask;
using Core.Shared.Data.Edit;
using Core.Shared.Data.Entities;
using Core.Shared.Enums;
using Core.Shared.Services.Features;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive;
using System.Reactive.Linq;
using Comment = Core.Shared.Data.Entities.Comment;
using SubTask = Core.Shared.Data.Entities.SubTask;

public class FeaturePageViewModel : ViewModelBase, IRoutableViewModel
{
    private readonly Feature?         m_feature;
    private readonly Guid             m_projectId;
    private readonly Lazy<IScreen>    m_hostScreen;
    private readonly IFeaturesService m_featuresService;

    public FeaturePageViewModel(Feature? feature, Guid projectId, Lazy<IScreen> hostScreen, IFeaturesService featuresService)
    {
        m_feature = feature;
        m_projectId = projectId;
        m_hostScreen = hostScreen;
        m_featuresService = featuresService;
        Name = feature?.Name ?? "Название идеи";
        Deadline = feature?.Deadline ?? DateTime.Now;
        
        var subTasks = feature?.TasksList.Select(CreateSubTaskViewModel) ?? [];

        SubTasksList = new ObservableCollection<SubTaskViewModel>(subTasks);
        if (feature is null) IsNameEditing = true;

        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync);
    }

    public string UrlPathSegment => "feature";

    public IScreen HostScreen => m_hostScreen.Value;

    [Reactive]
    public string Name { get; set; }
    
    [Reactive]
    public bool IsNameEditing { get; set; }

    public ObservableCollection<SubTaskViewModel> SubTasksList { get; }

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
    
    public DateTime Deadline { get; set; }
    
    public string Description { get; set; } = string.Empty;
    
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }

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
        Observable.Return<IReadOnlyList<User>>(
            [
                new User
                {
                    Email = "asd@asd",
                    Name = "asdadad",
                    Role = ERole.Employee,
                }
            ]
        ),
        it => SubTasksList.Remove(it)
    );

    private async Task SaveAsync()
    {
        var editData = CreateEditData();

        if (m_feature is null)
        {
            await m_featuresService.AddAsync(editData);
            
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
        Deadline = Deadline,
        Description = Description
    };
}