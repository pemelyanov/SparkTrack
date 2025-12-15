namespace SparkTrack.Core.Services.Features;

using Authorization;
using Exceptions;
using Repositories;
using Shared.Data;
using Shared.Data.Entities;
using Shared.Enums;

internal class FeaturesService(IFeaturesRepository featuresRepository, IAuthorizationService authorizationService) : IFeaturesService
{
    public Task<IReadOnlyPagedData<Feature>> GetPageAsync(Guid? projectId, bool showCompleted, PageQuery pageQuery)
    {
        if (authorizationService.CurrentUser is null) throw new UnauthorizedException();

        Guid? employeeFilter = authorizationService.CurrentUser.Role is ERole.Employee
            ? authorizationService.CurrentUser.Id
            : null;

        return featuresRepository.GetPageAsync(projectId, showCompleted, employeeFilter, pageQuery);
    }

    public Task<Feature> GetAsync(int id) => throw new NotImplementedException();
}