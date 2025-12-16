namespace SparkTrack.WebAPI.Controllers;

using Core.Services.Features;
using Core.Shared.Data;
using DTO;
using DTO.Edit;
using Extensions;
using MappingExtensions;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("features")]
public class FeaturesController(IFeaturesService featuresService) : Controller
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyPagedData<FeatureDTO>>> GetListAsync(
        Guid? projectId = null,
        bool showCompleted = false,
        [FromQuery] PageQueryDTO? pageQuery = null
    )
    {
        var page = await featuresService.GetPageAsync(projectId, showCompleted, pageQuery?.ToDomain() ?? PageQuery.All);

        return Ok(page);
    }

    [HttpPost]
    public Task<ActionResult> AddAsync(FeatureEditDTO featureEdit)
    {
        return this.CreatedWithDomainExceptionsHandling(() => featuresService.AddAsync(featureEdit.ToDomain()));
    }
}