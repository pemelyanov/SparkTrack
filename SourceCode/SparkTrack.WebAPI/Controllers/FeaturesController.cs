namespace SparkTrack.WebAPI.Controllers;

using Core.Shared.Data;
using Core.Shared.Enums;
using Core.Shared.Services.Features;
using DTO;
using DTO.Edit;
using Extensions;
using MappingExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("features")]
public class FeaturesController(IFeaturesService featuresService) : Controller
{
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FeatureDTO>> GetAsync(
        [FromRoute] int id
    )
    {
        var feature = await featuresService.GetAsync(id);

        return feature is null ? NotFound() : Ok(feature.ToDTO());
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
    [Authorize(Roles = nameof(ERole.Admin))]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<ActionResult<int>> AddAsync(FeatureEditDTO featureEdit)
    {
        return this.CreatedWithDomainExceptionsHandling(
            () => featuresService.AddAsync(featureEdit.ToDomain())
        );
    }

    [HttpPatch]
    [Authorize(Roles = nameof(ERole.Admin))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<ActionResult> EditAsync(FeatureEditDTO featureEdit)
    {
        return this.OkWithDomainExceptionsHandling(() => featuresService.EditAsync(featureEdit.ToDomain()));
    }
}