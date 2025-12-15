using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SparkTrack.AvaloniaImpl.Controls.FeatureStatus;

using Core.Shared.Data.Entities;

public partial class FeatureStatus : UserControl
{
    public FeatureStatus()
    {
        InitializeComponent();
    }

    #region SubTasksList Property

    public static readonly StyledProperty<IReadOnlyList<SubTask>> SubTasksListProperty =
        AvaloniaProperty.Register<FeatureStatus, IReadOnlyList<SubTask>>(nameof(SubTasksList));

    public IReadOnlyList<SubTask> SubTasksList
    {
        get => GetValue(SubTasksListProperty);
        set => SetValue(SubTasksListProperty, value);
    }

    #endregion
}