namespace SparkTrack.AvaloniaImpl.Pages.Feature;

using System.Collections.ObjectModel;
using Controls.Comment;
using Controls.SubTask;
using Core.Shared.Data.Entities;
using Core.Shared.Enums;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive.Linq;
using Comment = Core.Shared.Data.Entities.Comment;
using SubTask = Core.Shared.Data.Entities.SubTask;

public class FeaturePageViewModel : ViewModelBase, IRoutableViewModel
{
    private readonly Lazy<IScreen> m_hostScreen;

    public FeaturePageViewModel(Feature? feature, Lazy<IScreen> hostScreen)
    {
        m_hostScreen = hostScreen;
        Name = feature?.Name ?? "Название идеи";
        
        var subTasks = feature?.TasksList.Select(CreateSubTaskViewModel) ?? [];

        SubTasksList = new ObservableCollection<SubTaskViewModel>(subTasks);
        if (feature is null) IsNameEditing = true;
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
}