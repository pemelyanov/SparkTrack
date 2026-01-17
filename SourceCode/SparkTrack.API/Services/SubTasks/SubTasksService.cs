namespace SparkTrack.API.Services.SubTasks;

using Core.Shared.Data.Entities;
using Core.Shared.Services.SubTasks;
using Delegates;
using MappingExtensions;
using EPaymentStatus = EPaymentStatus;

public class SubTasksService(ClientFactory<SubTasksClient> subTasksClientFactory) : ISubTasksService
{
    public async Task<SubTask?> SetIsCompletedAsync(Guid id, bool value, Guid currentVersion)
    {
        using var wrapper = subTasksClientFactory();

        var dto = await wrapper.Client.SetIsCompletedAsync(id, value, currentVersion);

        return dto?.ToDomain();
    }

    public async Task<SubTask?> SetPaymentStatusAsync(Guid id, Core.Shared.Enums.EPaymentStatus value, Guid currentVersion)
    {
        using var wrapper = subTasksClientFactory();

        var dto = await wrapper.Client.SetPaymentStatusAsync(id, value.Cast<EPaymentStatus>(), currentVersion);

        return dto?.ToDomain();
    }
}