using Avalonia.Controls;

namespace SparkTrack.AvaloniaImpl.Pages.Users;

using Avalonia.Input;
using Avalonia.ReactiveUI;
using Core.Shared.Data.Entities;
using ReactiveUI;
using ViewModels;

[SingleInstanceView]
public partial class UsersPage : ReactiveUserControl<UsersPageViewModel>
{
    public UsersPage()
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
        
        
    }
}