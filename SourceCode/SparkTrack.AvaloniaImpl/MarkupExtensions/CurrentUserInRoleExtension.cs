namespace SparkTrack.AvaloniaImpl.MarkupExtensions;

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
        if (s_authorizationService.CurrentUser.Value is not { } user) return false;

        var intersection = user.Role & role;

        if (role is ERole.Employee) { }

        return Inverse ? intersection == 0 : intersection != 0;
    }
}