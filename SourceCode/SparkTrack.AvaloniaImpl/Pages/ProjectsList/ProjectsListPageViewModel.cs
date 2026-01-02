namespace SparkTrack.AvaloniaImpl.Pages.ProjectsList;

using Core.Shared.Data.Entities;
using Core.Shared.Services.Projects;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive;
using System.Reactive.Disposables;
using ViewModels;

public class ProjectsListPageViewModel : ViewModelBase, IRoutableViewModel
{
    private readonly Lazy<IScreen>    m_hostScreen;
    private readonly IProjectsService m_projectsService;

    public ProjectsListPageViewModel(Lazy<IScreen> hostScreen, IProjectsService projectsService)
    {
        m_hostScreen = hostScreen;
        m_projectsService = projectsService;

        ReloadTableCommand = ReactiveCommand.CreateFromTask(ReloadTableAsync);
    }

    public string UrlPathSegment => "projects";

    public IScreen HostScreen => m_hostScreen.Value;

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);

        ReloadTableCommand.Execute().Subscribe().DisposeWith(disposables);
    }

    [Reactive]
    public IReadOnlyList<SelectableViewModel<Project>> ProjectsList { get; private set; } = [];
    
    public ReactiveCommand<Unit, Unit> ReloadTableCommand { get; }

    private async Task ReloadTableAsync()
    {
        var projects = await m_projectsService.GetListAsync();

        ProjectsList = projects.Select(it => new SelectableViewModel<Project>(it)).ToArray();
    }
}