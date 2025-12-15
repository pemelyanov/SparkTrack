namespace SparkTrack.AvaloniaImpl.Pages.Features;

using System.Reactive.Disposables;
using System.Reactive.Linq;
using Core.Shared.Data.Entities;
using Core.Shared.Enums;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

public class FeaturesPageViewModel(Lazy<IScreen> screen) : ViewModelBase, IRoutableViewModel
{
    protected override void OnFirstActivated(CompositeDisposable disposables)
    {
        base.OnFirstActivated(disposables);
        
        SetupItemSelectionChangedReaction(disposables);
        SetupTableSelectionStateChangeReaction(disposables);
    }

    public string UrlPathSegment => "features";

    public IScreen HostScreen => screen.Value;
    
    [Reactive]
    public bool? CurrentPageSelectionState { get; set; }

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
    
    private void SetupTableSelectionStateChangeReaction(CompositeDisposable disposables)
    {
        this.WhenAnyValue(vm => vm.CurrentPageSelectionState)
            .Where(it => it is not null)
            .Subscribe(
                state =>
                {
                    foreach (var item in CurrentPageData)
                        item.IsSelected = state is true;
                }
            )
            .DisposeWith(disposables);
    }
    
    private void SetupItemSelectionChangedReaction(CompositeDisposable disposables)
    {
        this.WhenAnyValue(it => it.CurrentPageData)
            .Select(
                list => list.Count == 0
                    ? Observable.Return(Array.Empty<bool>())
                    : list.Select(it => it.WhenAnyValue(vm => vm.IsSelected)).CombineLatest()
            )
            .Switch()
            .Select<IList<bool>, bool?>(
                selectionList =>
                {
                    if (selectionList.Count == 0) return false;

                    var selectedQuantity = 0;
                    var unselectedQuantity = 0;

                    foreach (bool isSelected in selectionList)
                        if (isSelected)
                            selectedQuantity++;
                        else unselectedQuantity++;

                    if (selectedQuantity == selectionList.Count) return true;
                    if (unselectedQuantity == selectionList.Count) return false;

                    return null;
                }
            )
            .Subscribe(value => CurrentPageSelectionState = value)
            .DisposeWith(disposables);
    }

}