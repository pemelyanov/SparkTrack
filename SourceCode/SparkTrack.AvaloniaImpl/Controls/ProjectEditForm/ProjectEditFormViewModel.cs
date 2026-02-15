namespace SparkTrack.AvaloniaImpl.Controls.ProjectEditForm;

using Core.Shared.Data.Entities;
using Core.Shared.Services.Projects;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Reactive;
using System.Reactive.Linq;
using ViewModels;

public class ProjectEditFormViewModel : DialogViewModelBase
{
    private readonly Project?         m_project;
    private readonly IProjectsService m_projectsService;

    public ProjectEditFormViewModel(Project? project, IProjectsService projectsService)
    {
        m_project = project;
        m_projectsService = projectsService;
        Name = project?.Name ?? string.Empty;
        Link = project?.Link ?? string.Empty;

        var canSave = this.WhenAnyValue(it => it.Name)
            .Select(it => !string.IsNullOrWhiteSpace(it));

        if (IsEditMode)
        {
            SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync, canSave);
            return;
        }
        
        SaveCommand = ReactiveCommand.CreateFromTask(CreateAsync, canSave);
    }

    public bool IsEditMode => m_project is not null;
    
    [Reactive]
    public string Name { get; set; }

    [Reactive]
    public string Link { get; set; }

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }

    public void Reset()
    {
        Name = string.Empty;
        Link = string.Empty;
    }
    
    private async Task SaveAsync()
    {
        var projectData = MapToProject();

        await m_projectsService.EditAsync(projectData);
        
        Close(true);
    }

    private async Task CreateAsync()
    {
        var projectData = MapToProject();

        await m_projectsService.AddAsync(projectData);
        
        Close(true);
    }

    private Project MapToProject()
    {
        return new Project
        {
            Id = m_project?.Id ?? Guid.Empty,
            Name = Name,
            Link = Link
        };
    }
}