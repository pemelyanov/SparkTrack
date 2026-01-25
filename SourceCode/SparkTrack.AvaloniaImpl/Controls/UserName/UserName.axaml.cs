using Avalonia.Controls;

namespace SparkTrack.AvaloniaImpl.Controls.UserName;

using Avalonia;
using Core.Shared.Data.Entities;

public partial class UserName : UserControl
{
    public UserName()
    {
        InitializeComponent();
    }

    #region User Property

    public static readonly StyledProperty<User> UserProperty =
        AvaloniaProperty.Register<UserName, User>(nameof(User));

    public User User
    {
        get => GetValue(UserProperty);
        set => SetValue(UserProperty, value);
    }

    #endregion
}