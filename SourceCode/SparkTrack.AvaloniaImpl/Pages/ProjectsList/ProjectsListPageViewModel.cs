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
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using ConfirmationOptions;
using Extensions;
using Reactive;
using ViewModels;

public class ProjectsListPageViewModel : ViewModelBase, IRoutableViewModel
{
    private readonly Lazy<IScreen>                                     m_hostScreen;
    private readonly IProjectsService                                  m_projectsService;
    private readonly Func<Project?, ProjectEditFormViewModel>          m_projectEditFormFactory;
    private readonly IDialogService                                    m_dialogService;
    private readonly BehaviorObservableSubject<IReadOnlyList<Project>> m_selectedProjects = new([]);

    public ProjectsListPageViewModel(
        Lazy<IScreen> hostScreen,
        IProjectsService projectsService,
        Func<Project?, ProjectEditFormViewModel> projectEditFormFactory,
        IDialogService dialogService
    )
    {
        m_hostScreen = hostScreen;
        m_projectsService = projectsService;
        m_projectEditFormFactory = projectEditFormFactory;
        m_dialogService = dialogService;

        ReloadTableCommand = ReactiveCommand.CreateFromTask(ReloadTableAsync);
        DeleteCommand = ReactiveCommand.CreateFromTask(DeleteAsync, m_selectedProjects.Select(it => it.Count > 0));
    }

    public string UrlPathSegment => "projects";

    public IScreen HostScreen => m_hostScreen.Value;

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);

        this.SetupSelectionList(it => it.ProjectsList, m_selectedProjects)
            .DisposeWith(disposables);

        ReloadTableCommand.Execute().Subscribe().DisposeWith(disposables);
    }

    [Reactive]
    public IReadOnlyList<SelectableViewModel<Project>> ProjectsList { get; private set; } = [];

    public ReactiveCommand<Unit, Unit> ReloadTableCommand { get; }

    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }

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

        if (await m_dialogService.ShowAsync(projectFormViewModel) is not true) return;

        await ReloadTableCommand.Execute().ToTask();
    }

    private async Task ReloadTableAsync()
    {
        var projects = await m_projectsService.GetListAsync();

        ProjectsList = projects.Select(it => new SelectableViewModel<Project>(it)).ToArray();
    }

    private async Task DeleteAsync()
    {
        if (m_selectedProjects.Value.Count == 0) return;

        var forceOption = new ForceDeleteOption();

        if (!await m_dialogService.ConfirmAsync(
            $"Вы уверены что хотите удалить выбранные проекты ({m_selectedProjects.Value.Count})? Проекты, имеющие связь с оплаченными задачами, будут добавлены в архив, остальные будут полностью удалены.",
            "Удаление проектов",
            additionalOptionsList: [forceOption]
        )) return;

        var errorsList = new List<(Exception exception, Project project)>();

        foreach (var project in m_selectedProjects.Value)
        {
            try
            {
                await m_projectsService.DeleteAsync(project.Id, forceOption.IsSelected);
            }
            catch (Exception e)
            {
                errorsList.Add((e, project));
            }
        }

        if (errorsList.Count != 0)
        {
            await m_dialogService.NotifyAsync(
                $"{string.Join(";\n\n\n", errorsList.Select(it => $"{it.project.Name}: {it.exception.Message}"))}.",
                "При удалении некоторых проектов возникли ошибки"
            );
        }

        await ReloadTableCommand.Execute().ToTask();
    }
}