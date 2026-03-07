namespace SparkTrack.AvaloniaImpl.Controls.Paginator;

using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.ReactiveUI;
using ReactiveUI;

public partial class Paginator : ReactiveUserControl<Paginator>
{
    public Paginator()
    {
        InitializeComponent();

        this.WhenActivated(disposables =>
        {
            this.WhenAnyValue(c => c.PagesQuantity)
                .Where(quantity => quantity < CurrentPage)
                .Where(quantity => quantity > 0)
                .Subscribe(quantity => CurrentPage = quantity)
                .DisposeWith(disposables);
        });

        CanNext = this.WhenAnyValue(c => c.CurrentPage, c => c.PagesQuantity)
            .Select(args => args.Item1 < args.Item2);

        CanPrevious = this.WhenAnyValue(c => c.CurrentPage)
            .Select(args => args > 1);
    }

    #region PagesQuantity

    public int PagesQuantity
    {
        get => GetValue(PagesQuantityProperty);
        set => SetValue(PagesQuantityProperty, value);
    }

    public static readonly StyledProperty<int> PagesQuantityProperty =
        AvaloniaProperty.Register<Paginator, int>(nameof(PagesQuantity), defaultBindingMode: BindingMode.OneWay);

    #endregion

    #region CurrentPage

    public int CurrentPage
    {
        get => GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    public static readonly StyledProperty<int> CurrentPageProperty =
        AvaloniaProperty.Register<Paginator, int>(nameof(CurrentPage), defaultValue: 1,
            defaultBindingMode: BindingMode.OneWayToSource);

    #endregion

    #region ItemsPerPage

    public int ItemsPerPage
    {
        get => GetValue(ItemsPerPageProperty);
        set => SetValue(ItemsPerPageProperty, value);
    }

    public static readonly StyledProperty<int> ItemsPerPageProperty =
        AvaloniaProperty.Register<Paginator, int>(nameof(ItemsPerPage), defaultValue: 25,
            defaultBindingMode: BindingMode.TwoWay);

    #endregion

    #region ItemsPerPageList

    public IReadOnlyList<int> ItemsPerPageList
    {
        get => GetValue(ItemsPerPageListProperty);
        set => SetValue(ItemsPerPageListProperty, value);
    }

    public static readonly StyledProperty<IReadOnlyList<int>> ItemsPerPageListProperty =
        AvaloniaProperty.Register<Paginator, IReadOnlyList<int>>(nameof(ItemsPerPageList),
            defaultValue: [10, 25, 50, 100,]
        );

    #endregion

    #region CanNext

    public IObservable<bool> CanNext
    {
        get => GetValue(CanNextProperty);
        private set => SetValue(CanNextProperty, value);
    }

    public static readonly StyledProperty<IObservable<bool>> CanNextProperty =
        AvaloniaProperty.Register<Paginator, IObservable<bool>>(nameof(CanNext));

    #endregion

    #region CanPrevious

    public IObservable<bool> CanPrevious
    {
        get => GetValue(CanPreviousProperty);
        private set => SetValue(CanPreviousProperty, value);
    }

    public static readonly StyledProperty<IObservable<bool>> CanPreviousProperty =
        AvaloniaProperty.Register<Paginator, IObservable<bool>>(nameof(CanPrevious));

    #endregion

    private void FirstPage_OnClick(object? sender, RoutedEventArgs e)
    {
        CurrentPage = 1;
    }

    private void PreviousPage_Click(object? sender, RoutedEventArgs e)
    {
        CurrentPage--;
    }

    private void NextPage_Click(object? sender, RoutedEventArgs e)
    {
        CurrentPage++;
    }

    private void LastPage_Click(object? sender, RoutedEventArgs e)
    {
        CurrentPage = PagesQuantity;
    }
}