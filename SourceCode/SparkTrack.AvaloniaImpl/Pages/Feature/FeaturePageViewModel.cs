namespace SparkTrack.AvaloniaImpl.Pages.Feature;

using System.Collections.ObjectModel;
using Controls.Comment;
using Controls.SubTask;
using Core.Shared.Data.Entities;
using Core.Shared.Enums;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using Comment = Core.Shared.Data.Entities.Comment;

public class FeaturePageViewModel(Lazy<IScreen> hostScreen) : ViewModelBase, IRoutableViewModel
{
    public string UrlPathSegment => "feature";

    public IScreen HostScreen => hostScreen.Value;

    public ObservableCollection<SubTaskViewModel> SubTasksList { get; } = [];

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
        var subTask = new SubTaskViewModel(
            [
                new User
                {
                    Email = "asd@asd",
                    Name = "asdadad",
                    Role = ERole.Employee,
                }
            ],
            it => SubTasksList.Remove(it)
        )
        {
            IsInEditMode = true
        };

        foreach (var taskViewModel in SubTasksList)
            taskViewModel.IsInEditMode = false;

        SubTasksList.Add(subTask);
    }
}