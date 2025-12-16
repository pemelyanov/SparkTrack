namespace SparkTrack.WebAPI.Controllers;

using Core.Shared.Data;
using Core.Shared.Services.Features;
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
    public async Task<ActionResult<PagedDTO<FeatureDTO>>> GetPageAsync(
        Guid? projectId = null,
        bool showCompleted = false,
        [FromQuery] PageQueryDTO? pageQuery = null
    )
    {
        var page = await featuresService.GetPageAsync(projectId, showCompleted, pageQuery?.ToDomain() ?? PageQuery.All);

        var mappedPage = page.ToDTO(it => it.ToDTO());

        return Ok(mappedPage);
    }

    [HttpPost]
    public Task<ActionResult> AddAsync(FeatureEditDTO featureEdit)
    {
        return this.CreatedWithDomainExceptionsHandling(() => featuresService.AddAsync(featureEdit.ToDomain()));
    }
}