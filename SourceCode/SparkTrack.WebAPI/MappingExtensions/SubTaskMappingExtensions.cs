namespace SparkTrack.WebAPI.MappingExtensions;

using Core.Shared.Data.Edit;
using DTO.Edit;

public static class SubTaskMappingExtensions
{
    public static SubTaskEditDTO ToDTO(this SubTaskEdit it) => new()
    {
        Id = it.Id,
        Name = it.Name,
        ExecutorEmployeeId = it.ExecutorEmployeeId,
        Cost = it.Cost,
        IsCompleted = it.IsCompleted,
        OnPayment = it.OnPayment
    };

    public static SubTaskEdit ToDomain(this SubTaskEditDTO it) => new()
    {
        Id = it.Id,
        Name = it.Name,
        ExecutorEmployeeId = it.ExecutorEmployeeId,
        Cost = it.Cost,
        IsCompleted = it.IsCompleted,
        OnPayment = it.OnPayment
    };
}