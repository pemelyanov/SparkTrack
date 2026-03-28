using Avalonia.Controls;

namespace SparkTrack.AvaloniaImpl.Pages.AdminFinance.Tabs.PendingPayments;

using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using Core.Client.Services.Configuration;
using Data.Configurations;
using Extensions;
using Splat;
using ViewModels;

public partial class PendingPayments : ReactiveUserControl<PendingPaymentsViewModel>
{
    private readonly IConfigurationService<AdminPendingPaymentsPageConfig> m_pageConfig =
        Locator.Current.GetService<IConfigurationService<AdminPendingPaymentsPageConfig>>()!;
    
    public PendingPayments()
    {
        InitializeComponent();
        
        PendingPaymentsTable.InitializeColumnsWidth(m_pageConfig.Config.ColumnWidths);
    }
    
    private void DataGrid_OnLoadingRow(object? sender, DataGridRowEventArgs e)
    {
        e.Row.Tapped += RowOnTapped;
    }
    
    private void DataGrid_OnUnloadingRow(object? sender, DataGridRowEventArgs e)
    {
        e.Row.Tapped -= RowOnTapped;
    }

    private void RowOnTapped(object? sender, TappedEventArgs e)
    {
        if(sender is not Control { DataContext: SelectableViewModel<PaymentBillViewModel> itemViewModel }) return;

        itemViewModel.IsSelected = !itemViewModel.IsSelected;
    }

    private void PendingPaymentsTable_OnUnloaded(object? sender, RoutedEventArgs e)
    { 
        PendingPaymentsTable.SaveColumnsWidth(m_pageConfig);
    }
}