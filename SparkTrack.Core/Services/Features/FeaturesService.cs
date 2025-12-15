namespace SparkTrack.Core.Services.Features;

using Authorization;
using Extensions;
using Repositories;
using Shared.Data;
using Shared.Data.Edit;
using Shared.Data.Entities;
using Shared.Enums;

internal class FeaturesService(IFeaturesRepository featuresRepository, IAuthorizationService authorizationService)
    : IFeaturesService
{
    public Task<IReadOnlyPagedData<Feature>> GetPageAsync(
        Guid? projectId,
        bool showCompleted,
        PageQuery pageQuery
    )
    {
        var currentUser = authorizationService.GetUserOrThrowIfUnauthorized();

        Guid? employeeFilter = currentUser.GetEmployeeIdOrNull();

        return featuresRepository.GetPageAsync(projectId, showCompleted, employeeFilter, pageQuery);
    }

    public Task<Feature> GetAsync(int id)
    {
        var currentUser = authorizationService.GetUserOrThrowIfUnauthorized();

        Guid? employeeFilter = currentUser.GetEmployeeIdOrNull();

        return featuresRepository.GetAsync(id, employeeFilter);
    }

    public Task AddAsync(FeatureEdit feature)
    {
        authorizationService.GetUserOrThrowIfNotInRole(ERole.Admin);

        return featuresRepository.AddAsync(feature);
    }

    public Task DeleteAsync(int id)
    {
        authorizationService.GetUserOrThrowIfNotInRole(ERole.Admin);

        return featuresRepository.DeleteAsync(id);
    }
}