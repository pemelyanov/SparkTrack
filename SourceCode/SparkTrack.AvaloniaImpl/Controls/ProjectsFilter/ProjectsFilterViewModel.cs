namespace SparkTrack.AvaloniaImpl.Controls.ProjectsFilter;

using Core.Shared.Data.Entities;
using Core.Shared.Services.Projects;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive;
using System.Reactive.Disposables;

public class ProjectsFilterViewModel : ViewModelBase
{
    private readonly IProjectsService m_projectsService;

    public ProjectsFilterViewModel(IProjectsService projectsService)
    {
        m_projectsService = projectsService;

        LoadListCommand = ReactiveCommand.CreateFromTask(LoadListAsync);
    }

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);

        LoadListCommand.Execute().Subscribe().DisposeWith(disposables);
    }

    [Reactive]
    public IReadOnlyList<Project> ProjectsList { get; private set; } = [];
    
    [Reactive]
    public Project? SelectedProject { get; set; }
    
    public ReactiveCommand<Unit, Unit> LoadListCommand { get; }

    private async Task LoadListAsync()
    {
        var projectsList = await m_projectsService.GetListAsync();

        ProjectsList = projectsList;
    }
}