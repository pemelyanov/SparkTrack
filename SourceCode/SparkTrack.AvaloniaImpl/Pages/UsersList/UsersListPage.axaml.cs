namespace SparkTrack.AvaloniaImpl.Pages.UsersList;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using ReactiveUI;
using ViewModels;
using Core.Shared.Data.Entities;

[SingleInstanceView]
public partial class UsersListPage : ReactiveUserControl<UsersListPageViewModel>
{
    public UsersListPage()
    {
        InitializeComponent();
    }
    
    private void DataGrid_OnSorting(object? sender, DataGridColumnEventArgs e)
    {
        throw new NotImplementedException();
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
        if(sender is not Control { DataContext: SelectableViewModel<User> userViewModel }) return;

        ViewModel?.OpenUserEditAsync(userViewModel.Model);
    }
}