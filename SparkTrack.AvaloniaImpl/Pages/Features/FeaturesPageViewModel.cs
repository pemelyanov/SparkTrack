namespace SparkTrack.AvaloniaImpl.Pages.Features;

using Core.Shared.Data.Entities;
using Core.Shared.Enums;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

public class FeaturesPageViewModel(Lazy<IScreen> screen) : ViewModelBase, IRoutableViewModel
{
    public string UrlPathSegment => "features";

    public IScreen HostScreen => screen.Value;

    [Reactive]
    public IReadOnlyList<FeatureViewModel> CurrentPageData { get; private set; } = Enumerable.Range(1, 25)
        .Select(
            it => new Feature()
            {
                Id = it,
                Name = $"Идея {it}",
                Deadline = DateTime.Now,
                Project = new Project
                {
                    Id = Guid.Empty,
                    Name = $"Проект {it}"
                },
                TasksList = [
                    new SubTask
                    {
                        Name = "Монтаж",
                        ExecutorEmployee = new User
                        {
                            Id = Guid.Empty,
                            Name = "Костя",
                            Role = ERole.Employee
                        },
                    },
                    new SubTask
                    {
                        Name = "Съемка",
                        ExecutorEmployee = new User
                        {
                            Id = Guid.Empty,
                            Name = "Влад",
                            Role = ERole.Employee
                        },
                    },
                    new SubTask
                    {
                        Name = "Превью",
                        ExecutorEmployee = new User
                        {
                            Id = Guid.Empty,
                            Name = "Олег",
                            Role = ERole.Employee
                        },
                    }
                ]
            }
        )
        .Select(it => new FeatureViewModel(it))
        .ToArray();
}