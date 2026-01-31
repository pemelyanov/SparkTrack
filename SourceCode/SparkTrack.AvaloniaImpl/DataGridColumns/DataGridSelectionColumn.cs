namespace SparkTrack.AvaloniaImpl.DataGridColumns;

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using DynamicData.Binding;
using ReactiveUI;
using System.Collections;
using System.Collections.Specialized;
using System.Reactive.Linq;

public class DataGridSelectionColumn : DataGridBoundColumn
{
    // HACK: Внутренний чекбокс для изменения и получения значения в ячейках. При использовании
    // нужно установить в DataContext нужный айтем и выполнить привязку Binding.
    // После выполнения всех операций биндинг нужно задиспоузить
    private readonly CheckBox     m_tempCellCheckBox = new();
    private          CheckBox?    m_headerCheckBox;
    private          bool         m_updatingByHeaderValue;
    private          bool         m_updatingByCellValue;
    private          IDisposable? m_itemsSourceSubscription;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Avalonia.Controls.DataGridTextColumn" /> class.
    /// </summary>
    public DataGridSelectionColumn()
    {
        BindingTarget = ToggleButton.IsCheckedProperty;
        CanUserReorder = false;
        CanUserResize = false;
        CanUserSort = false;

        HeaderTemplate = new FuncDataTemplate(
            _ => true,
            (_, _) =>
            {
                var checkBox = new CheckBox
                {
                    Name = "HeaderSelectionCheckBox"
                };

                checkBox.IsCheckedChanged += HeaderCheckBox_IsCheckedChanged;
                checkBox.Bind(InputElement.IsEnabledProperty, GetItemsSourceIsNotEmptyObservable());
                
                if(m_itemsSourceSubscription is null)
                    m_itemsSourceSubscription = GetItemsSourceIsNotEmptyObservable().Subscribe(_ => OnCellValueUpdated());

                m_headerCheckBox = checkBox;

                return checkBox;
            }
        );
    }

    /// <summary>
    /// Causes the column cell being edited to revert to the specified value.
    /// </summary>
    /// <param name="editingElement">The element that the column displays for a cell in editing mode.</param>
    /// <param name="uneditedValue">The previous, unedited value in the cell being edited.</param>
    protected override void CancelCellEdit(Control editingElement, object uneditedValue)
    {
        if (!(editingElement is CheckBox checkBox))
            return;
        if (!(uneditedValue is bool value))
            value = false;
        checkBox.IsChecked = value;
    }

    /// <summary>
    /// Gets a <see cref="T:Avalonia.Controls.TextBox" /> control that is bound to the column's <see cref="P:Avalonia.Controls.DataGridBoundColumn.Binding" /> property value.
    /// </summary>
    /// <param name="cell">The cell that will contain the generated element.</param>
    /// <param name="dataItem">The data item represented by the row that contains the intended cell.</param>
    /// <returns>A new <see cref="T:Avalonia.Controls.TextBox" /> control that is bound to the column's <see cref="P:Avalonia.Controls.DataGridBoundColumn.Binding" /> property value.</returns>
    protected override Control GenerateEditingElementDirect(DataGridCell cell, object dataItem)
    {
        var checkBox = new CheckBox
        {
            Name = "CellSelectionCheckBox",
        };

        return checkBox;
    }

    /// <summary>
    /// Gets a read-only <see cref="T:Avalonia.Controls.TextBlock" /> element that is bound to the column's <see cref="P:Avalonia.Controls.DataGridBoundColumn.Binding" /> property value.
    /// </summary>
    /// <param name="cell">The cell that will contain the generated element.</param>
    /// <param name="dataItem">The data item represented by the row that contains the intended cell.</param>
    /// <returns>A new, read-only <see cref="T:Avalonia.Controls.TextBlock" /> element that is bound to the column's <see cref="P:Avalonia.Controls.DataGridBoundColumn.Binding" /> property value.</returns>
    protected override Control GenerateElement(DataGridCell cell, object dataItem)
    {
        var checkBox = new CheckBox
        {
            Name = "CellSelectionCheckBox",
        };

        if (Binding != null)
            checkBox.Bind(ToggleButton.IsCheckedProperty, Binding);

        checkBox.IsCheckedChanged += CellCheckBox_OnIsCheckedChanged;

        return checkBox;
    }

    private void CellCheckBox_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        Dispatcher.UIThread.Post(
            OnCellValueUpdated);
    }

    private void OnCellValueUpdated()
    {
        if (Binding is null || m_headerCheckBox is null || m_updatingByHeaderValue) return;

        m_updatingByCellValue = true;
        var selectedQuantity = 0;
        var unselectedQuantity = 0;
        var totalQuantity = 0;

        if (OwningGrid.ItemsSource is null)
        {
            m_headerCheckBox.IsChecked = false;
            m_updatingByCellValue = false;
            return;
        }

        foreach (var item in OwningGrid.ItemsSource)
        {
            m_tempCellCheckBox.DataContext = item;

            var binding = m_tempCellCheckBox.Bind(ToggleButton.IsCheckedProperty, Binding);

            if (m_tempCellCheckBox.IsChecked is true)
                selectedQuantity++;
            else if (m_tempCellCheckBox.IsChecked is false)
                unselectedQuantity++;

            totalQuantity++;

            binding.Dispose();
        }

        m_headerCheckBox.IsChecked = selectedQuantity == totalQuantity
            ? true
            : unselectedQuantity == totalQuantity
                ? false
                : null;
        
        m_updatingByCellValue = false;
    }

    /// <summary>Called when the cell in the column enters editing mode.</summary>
    /// <param name="editingElement">The element that the column displays for a cell in editing mode.</param>
    /// <param name="editingEventArgs">Information about the user gesture that is causing a cell to enter editing mode.</param>
    /// <returns>The unedited value. </returns>
    protected override object? PrepareCellForEdit(
        Control editingElement,
        RoutedEventArgs editingEventArgs
    )
    {
        if (!(editingElement is CheckBox checkBox))
            return false;

        return checkBox.IsChecked;
    }

    private void HeaderCheckBox_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox headerCheckBox || Binding is null || headerCheckBox.IsChecked is null
            || m_updatingByCellValue) return;

        m_updatingByHeaderValue = true;

        foreach (var item in OwningGrid.ItemsSource)
        {
            m_tempCellCheckBox.DataContext = item;

            var binding = m_tempCellCheckBox.Bind(ToggleButton.IsCheckedProperty, Binding);

            m_tempCellCheckBox.IsChecked = headerCheckBox.IsChecked;

            binding.Dispose();
        }

        m_updatingByHeaderValue = false;
    }

    private IObservable<bool> GetItemsSourceIsNotEmptyObservable()
    {
        return OwningGrid.WhenAnyValue(it => it.ItemsSource)
            .Select(
                it => it is INotifyCollectionChanged notifyCollectionChanged
                    ? notifyCollectionChanged.ObserveCollectionChanges()
                        .Select(_ => notifyCollectionChanged as IEnumerable)
                        .StartWith(it)
                    : Observable.Return(it)
            )
            .Switch()
            .Select(
                itemsSource =>
                {
                    if (itemsSource is null) return false;

                    foreach (object? _ in itemsSource)
                        return true;

                    return false;
                }
            )
            .ObserveOn(RxApp.MainThreadScheduler);
    }
}