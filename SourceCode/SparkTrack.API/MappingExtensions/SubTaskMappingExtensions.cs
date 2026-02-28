namespace SparkTrack.API.MappingExtensions;

using API;
using Core.Shared.Data.Edit;
using Core.Shared.Data.Entities;

public static class SubTaskMappingExtensions
{
    public static SubTaskEditDTO ToDTO(this SubTaskEdit it) => new()
    {
        Id = it.Id,
        Name = it.Name,
        ExecutorEmployeeId = it.ExecutorEmployeeId,
        DependsOnIdList = it.DependsOnIdList,
        Deadline = it.Deadline.ToUniversalTime(),
        Cost = it.Cost,
        IsCompleted = it.IsCompleted,
        PaymentStatus = it.PaymentStatus.Cast<EPaymentStatus>(),
        Version = it.Version,
        TimelyBonus = it.TimelyBonus
    };

    public static SubTask ToDomain(this SubTaskDTO it) => new()
    {
        Id = it.Id,
        Name = it.Name,
        ExecutorEmployee = it.ExecutorEmployee.ToDomain(),
        DependsOnIdList = it.DependsOnIdList,
        Deadline = it.Deadline.ToLocalTime(),
        Cost = it.Cost,
        IsCompleted = it.IsCompleted,
        PaymentStatus = it.PaymentStatus.Cast<Core.Shared.Enums.EPaymentStatus>(),
        Version = it.Version,
        TimelyBonus = it.TimelyBonus,
        CompletedAt = it.CompletedAt == DateTime.MinValue ? null : it.CompletedAt?.ToLocalTime(),
        IsTimelyBonusApproved = it.IsTimelyBonusApproved
    };
}