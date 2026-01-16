using Avalonia;
using Avalonia.Controls;

namespace SparkTrack.AvaloniaImpl.Controls.EditDates;

public partial class EditDates : UserControl
{
    public EditDates()
    {
        InitializeComponent();
    }

    #region EditedAt Property

    public static readonly StyledProperty<DateTime?> EditedAtProperty =
        AvaloniaProperty.Register<EditDates, DateTime?>(nameof(EditedAt));

    public DateTime? EditedAt
    {
        get => GetValue(EditedAtProperty);
        set => SetValue(EditedAtProperty, value);
    }

    #endregion

    #region CreatedAt Property

    public static readonly StyledProperty<DateTime?> CreatedAtProperty =
        AvaloniaProperty.Register<EditDates, DateTime?>(nameof(CreatedAt));

    public DateTime? CreatedAt
    {
        get => GetValue(CreatedAtProperty);
        set => SetValue(CreatedAtProperty, value);
    }

    #endregion
}