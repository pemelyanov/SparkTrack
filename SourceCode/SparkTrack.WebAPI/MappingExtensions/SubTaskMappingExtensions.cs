namespace SparkTrack.WebAPI.MappingExtensions;

using Core.Shared.Data.Edit;
using Core.Shared.Data.Entities;
using DTO;
using DTO.Edit;

public static class SubTaskMappingExtensions
{
    public static SubTaskEdit ToDomain(this SubTaskEditDTO it) => new()
    {
        Id = it.Id,
        Name = it.Name,
        ExecutorEmployeeId = it.ExecutorEmployeeId,
        DependsOnIdList = it.DependsOnIdList,
        Cost = it.Cost,
        Deadline = it.Deadline,
        IsCompleted = it.IsCompleted,
        PaymentStatus = it.PaymentStatus,
        Version = it.Version,
        TimelyBonus = it.TimelyBonus
    };
    
    public static SubTaskDTO ToDTO(this SubTask it) => new()
    {
        Id = it.Id,
        Name = it.Name,
        ExecutorEmployee = it.ExecutorEmployee.ToDTO(),
        DependsOnIdList = it.DependsOnIdList,
        Deadline = it.Deadline,
        Cost = it.Cost,
        IsCompleted = it.IsCompleted,
        PaymentStatus = it.PaymentStatus,
        Version = it.Version,
        TimelyBonus = it.TimelyBonus,
        IsTimelyBonusApproved = it.IsTimelyBonusApproved,
        CompletedAt = it.CompletedAt
    };
}