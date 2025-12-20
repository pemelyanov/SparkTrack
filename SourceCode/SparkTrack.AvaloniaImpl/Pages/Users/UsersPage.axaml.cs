using Avalonia.Controls;

namespace SparkTrack.AvaloniaImpl.Pages.Users;

using Avalonia.ReactiveUI;

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
}