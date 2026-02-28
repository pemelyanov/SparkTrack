namespace SparkTrack.Core.Shared.Extensions;

using Enums;

public static class RoleExtensions
{
    public static ERole ResolveSubordinateRole(this ERole currentRole) => currentRole switch
    {
        ERole.Admin => ERole.Employee,
        ERole.God => ERole.Admin,
        _ => throw new NotSupportedException()
    };

    public static bool IsAnyRole(this ERole currentRole, ERole roles)
    {
        var intersection = currentRole & roles;

        return intersection != 0;
    }
}