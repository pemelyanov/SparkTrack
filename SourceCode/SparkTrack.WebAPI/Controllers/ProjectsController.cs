namespace SparkTrack.WebAPI.Controllers;

using Core.Shared.Enums;
using Core.Shared.Services.Projects;
using DTO;
using Extensions;
using MappingExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("projects")]
public class ProjectsController(IProjectsService projectsService) : Controller
{
    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<ProjectDTO>>> GetListAsync()
    {
        var list = await projectsService.GetListAsync();
        var mappedList = list.Select(it => it.ToDTO());
        
        return Ok(mappedList);
    }
    
    [HttpPost]
    [Authorize(Roles = nameof(ERole.God))]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<ActionResult> AddAsync(ProjectDTO projectDTO)
    {
        return this.CreatedWithDomainExceptionsHandling(
            () => projectsService.AddAsync(projectDTO.ToDomain())
        );
    }
    
    [HttpPatch]
    [Authorize(Roles = nameof(ERole.God))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult> EditAsync(ProjectDTO projectDTO)
    {
        return this.OkWithDomainExceptionsHandling(
            () => projectsService.EditAsync(projectDTO.ToDomain())
        );
    }
    
    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(ERole.God))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult> DeleteAsync([FromRoute] Guid id, [FromQuery] bool force)
    {
        return this.OkWithDomainExceptionsHandling(
            () => projectsService.DeleteAsync(id, force)
        );
    }
}