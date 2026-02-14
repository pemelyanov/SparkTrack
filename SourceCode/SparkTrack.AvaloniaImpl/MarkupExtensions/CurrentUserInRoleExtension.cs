namespace SparkTrack.AvaloniaImpl.MarkupExtensions;

using System.Reactive.Linq;
using Avalonia;
using Core.Client.Services.Authorization;
using Core.Shared.Enums;
using Splat;

public class CurrentUserInRoleExtension(ERole role)
{
    public bool Inverse { get; set; }

    private static readonly IAuthorizationService s_authorizationService =
        Locator.Current.GetService<IAuthorizationService>()!;

    public object ProvideValue()
    {
        return s_authorizationService.CurrentUser.Select(currentUser =>
                {
                    if (currentUser is not { } user) return false;

                    var intersection = user.Role & role;

                    if (role is ERole.Employee) { }

                    return Inverse ? intersection == 0 : intersection != 0;
                }
            )
            .ToBinding();
    }
}