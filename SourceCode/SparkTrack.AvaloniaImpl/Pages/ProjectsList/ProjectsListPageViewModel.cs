namespace SparkTrack.AvaloniaImpl.Pages.ProjectsList;

using Controls.ProjectEditForm;
using Core.Shared.Data.Entities;
using Core.Shared.Services.Projects;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Services.DialogHost;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Threading.Tasks;
using ViewModels;

public class ProjectsListPageViewModel : ViewModelBase, IRoutableViewModel
{
    private readonly Lazy<IScreen>                            m_hostScreen;
    private readonly IProjectsService                         m_projectsService;
    private readonly Func<Project?, ProjectEditFormViewModel> m_projectEditFormFactory;
    private readonly IDialogHost                              m_dialogHost;

    public ProjectsListPageViewModel(Lazy<IScreen> hostScreen, IProjectsService projectsService, Func<Project?, ProjectEditFormViewModel> projectEditFormFactory, IDialogHost dialogHost)
    {
        m_hostScreen = hostScreen;
        m_projectsService = projectsService;
        m_projectEditFormFactory = projectEditFormFactory;
        m_dialogHost = dialogHost;

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

    public async Task AddProjectAsync()
    {
        await OpenEditProjectForm(null);
    }

    public async Task EditProjectAsync(Project project)
    {
        await OpenEditProjectForm(project);
    }

    private async Task OpenEditProjectForm(Project? project)
    {
        var projectFormViewModel = m_projectEditFormFactory(project);

        if(await m_dialogHost.ShowAsync(projectFormViewModel) is not true) return;

        await ReloadTableCommand.Execute().ToTask();
    }

    private async Task ReloadTableAsync()
    {
        var projects = await m_projectsService.GetListAsync();

        ProjectsList = projects.Select(it => new SelectableViewModel<Project>(it)).ToArray();
    }
}