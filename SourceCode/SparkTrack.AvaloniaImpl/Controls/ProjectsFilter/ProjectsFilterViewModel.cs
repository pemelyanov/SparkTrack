namespace SparkTrack.AvaloniaImpl.Controls.ProjectsFilter;

using Core.Shared.Data.Entities;
using Core.Shared.Services.Projects;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

public class ProjectsFilterViewModel : ViewModelBase
{
    private readonly IProjectsService m_projectsService;
    private          Guid?            m_idToSelectOnNextUpdate;

    public ProjectsFilterViewModel(IProjectsService projectsService)
    {
        m_projectsService = projectsService;

        LoadListCommand = ReactiveCommand.CreateFromTask(LoadListAsync);
    }

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);

        LoadListCommand.Execute().Subscribe().DisposeWith(disposables);

        this.WhenAnyValue(it => it.ProjectsList)
            .Subscribe(list =>
                {
                    if(list.Count == 0) return;
                    
                    var idToSelectOnNextUpdate = m_idToSelectOnNextUpdate;
                    m_idToSelectOnNextUpdate = null;
                    
                    if (idToSelectOnNextUpdate is null) return;

                    SelectedProject = list.FirstOrDefault(it => it.Id == idToSelectOnNextUpdate);
                }
            )
            .DisposeWith(disposables);
    }

    [Reactive]
    public IReadOnlyList<Project> ProjectsList { get; private set; } = [];
    
    [Reactive]
    public Project? SelectedProject { get; set; }
    
    public ReactiveCommand<Unit, Unit> LoadListCommand { get; }

    public Guid? SelectedId => m_idToSelectOnNextUpdate ?? SelectedProject?.Id;
    
    public void AutoSelectOnceOnNextUpdate(Guid id)
    {
        m_idToSelectOnNextUpdate = id;
    }

    public IObservable<Guid?> SelectedIdChanged() => this.WhenAnyValue(it => it.SelectedProject)
        .Select(it => it?.Id)
        .StartWith(m_idToSelectOnNextUpdate)
        .DistinctUntilChanged();

    private async Task LoadListAsync()
    {
        var projectsList = await m_projectsService.GetListAsync();

        ProjectsList = projectsList;
    }
}