namespace SparkTrack.AvaloniaImpl.MarkupExtensions;

using Avalonia.Data;
using Avalonia.Data.Converters;
using Core.Client.Services.Authorization;
using Splat;

public class CurrentUserIdIsExtension(IBinding userIdBinding)
{
    private static readonly IAuthorizationService s_authorizationService =
        Locator.Current.GetService<IAuthorizationService>()!;

    public bool Inverse { get; set; }

    public object ProvideValue()
    {
        return new MultiBinding
        {
            Bindings = [userIdBinding],
            Converter = new FuncMultiValueConverter<Guid, bool>(
                values =>
                {
                    if (s_authorizationService.CurrentUser.Value is not { } user) return false;

                    var id = values.First();

                    return !Inverse ? id == user.Id : id != user.Id;
                }
            )
        };
    }
}