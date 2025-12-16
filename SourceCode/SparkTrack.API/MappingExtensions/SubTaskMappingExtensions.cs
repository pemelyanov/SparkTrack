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
    
    public static SubTaskDTO ToDTO(this SubTask it) => new()
    {
        Id = it.Id,
        Name = it.Name,
        ExecutorEmployee = it.ExecutorEmployee.ToDTO(),
        Cost = it.Cost,
        IsCompleted = it.IsCompleted,
        OnPayment = it.OnPayment
    };
    
    public static SubTask ToDomain(this SubTaskDTO it) => new()
    {
        Id = it.Id,
        Name = it.Name,
        ExecutorEmployee = it.ExecutorEmployee.ToDomain(),
        Cost = it.Cost,
        IsCompleted = it.IsCompleted,
        OnPayment = it.OnPayment
    };
}