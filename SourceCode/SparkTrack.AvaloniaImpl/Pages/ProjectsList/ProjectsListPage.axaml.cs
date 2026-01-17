namespace SparkTrack.AvaloniaImpl.Pages.ProjectsList;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using Core.Shared.Data.Entities;
using ReactiveUI;
using ViewModels;

[SingleInstanceView]
public partial class ProjectsListPage : ReactiveUserControl<ProjectsListPageViewModel>
{
    public ProjectsListPage()
    {
        InitializeComponent();
    }

    private void DataGrid_OnLoadingRow(object? sender, DataGridRowEventArgs e)
    {
        e.Row.DoubleTapped += RowOnDoubleTapped;
    }
    
    private void DataGrid_OnUnloadingRow(object? sender, DataGridRowEventArgs e)
    {
        e.Row.DoubleTapped -= RowOnDoubleTapped;
    }

    private void RowOnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if(sender is not Control { DataContext: SelectableViewModel<Project> projectViewModel }) return;
        
        ViewModel?.EditProjectAsync(projectViewModel.Model);
    }
}