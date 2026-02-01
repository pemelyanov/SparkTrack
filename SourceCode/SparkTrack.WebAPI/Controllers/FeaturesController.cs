namespace SparkTrack.WebAPI.Controllers;

using Core.Shared.Data;
using Core.Shared.Enums;
using Core.Shared.Services.Comments;
using Core.Shared.Services.Features;
using DTO;
using DTO.Edit;
using Extensions;
using MappingExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("features")]
public class FeaturesController(IFeaturesService featuresService, ICommentsService commentsService) : Controller
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
        DateTime? startDate = null,
        DateTime? endDate = null,
        [FromQuery] PageQueryDTO? pageQuery = null
    )
    {
        var page = await featuresService.GetPageAsync(
            projectId,
            showCompleted,
            startDate?.ToUniversalTime(),
            endDate?.ToUniversalTime(),
            pageQuery?.ToDomain() ?? PageQuery.All
        );

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
        return this.CreatedWithDomainExceptionsHandling(() => featuresService.AddAsync(featureEdit.ToDomain())
        );
    }

    [HttpPatch]
    [Authorize(Roles = nameof(ERole.Admin))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public Task<ActionResult> EditAsync(FeatureEditDTO featureEdit)
    {
        return this.OkWithDomainExceptionsHandling(() => featuresService.EditAsync(featureEdit.ToDomain()));
    }

    [HttpGet("{featureId}/comments")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedDTO<CommentDTO>>> GetCommentsPageAsync(
        [FromRoute] int featureId,
        [FromQuery] PageQueryDTO? pageQuery = null
    )
    {
        var page = await commentsService.GetPageAsync(featureId, pageQuery?.ToDomain() ?? PageQuery.All);

        var mappedPage = page.ToDTO(it => it.ToDTO());

        return Ok(mappedPage);
    }

    [HttpPost("{featureId}/comments")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public Task<ActionResult> AddCommentAsync([FromRoute] int featureId, CommentEditDTO commentEdit)
    {
        return this.CreatedWithDomainExceptionsHandling(() => commentsService.AddAsync(
                featureId,
                commentEdit.ToDomain()
            )
        );
    }

    [HttpPatch("comments")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<ActionResult<CommentDTO>> EditCommentAsync(CommentEditDTO commentEdit)
    {
        return this.OkWithDomainExceptionsHandling(async () =>
            {
                var comment = await commentsService.EditAsync(commentEdit.ToDomain());

                return comment?.ToDTO()!;
            }
        );
    }

    [HttpDelete("comments/{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<ActionResult> DeleteCommentAsync([FromRoute] Guid id)
    {
        return this.OkWithDomainExceptionsHandling(() => commentsService.DeleteAsync(id));
    }
}