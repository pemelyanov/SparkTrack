namespace SparkTrack.Core.Services.Features;

using Archive;
using Authorization;
using Extensions;
using Repositories;
using Shared.Data;
using Shared.Data.Edit;
using Shared.Data.Entities;
using Shared.Enums;
using Shared.Services.Features;

internal class FeaturesService(IFeaturesRepository featuresRepository, IAuthorizationService authorizationService, IFeatureArchiveService featureArchiveService)
    : IFeaturesService
{
    public Task<IReadOnlyPagedData<Feature>> GetPageAsync(
        Guid? projectId,
        bool showCompleted,
        DateTime? startDate,
        DateTime? endDate,
        PageQuery pageQuery
    )
    {
        var currentUser = authorizationService.GetUserOrThrowIfUnauthorized();

        Guid? employeeFilter = currentUser.GetEmployeeIdOrNull();

        return featuresRepository.GetPageAsync(projectId, showCompleted, employeeFilter, startDate, endDate, pageQuery);
    }

    public Task<Feature?> GetAsync(int id)
    {
        var currentUser = authorizationService.GetUserOrThrowIfUnauthorized();

        Guid? employeeFilter = currentUser.GetEmployeeIdOrNull();

        return featuresRepository.GetAsync(id, employeeFilter);
    }

    public Task<int> AddAsync(FeatureEdit feature)
    {
        return featuresRepository.AddAsync(feature);
    }

    public Task EditAsync(FeatureEdit feature)
    {
        return featuresRepository.EditAsync(feature);
    }

    public Task DeleteAsync(int id, bool force)
    {
        if(force) return featuresRepository.DeleteAsync(id);

        return featureArchiveService.ArchiveAsync(id, EArchiveSource.User);
    }
}