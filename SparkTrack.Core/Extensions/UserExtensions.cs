namespace SparkTrack.Core.Extensions;

using Shared.Data.Entities;
using Shared.Enums;

public static class UserExtensions
{
    public static Guid? GetEmployeeIdOrNull(this User user) => user.Role is ERole.Employee
        ? user.Id
        : null;
}