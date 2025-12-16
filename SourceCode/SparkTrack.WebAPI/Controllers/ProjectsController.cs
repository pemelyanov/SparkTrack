namespace SparkTrack.WebAPI.Controllers;

using Core.Shared.Services.Projects;
using DTO;
using Extensions;
using MappingExtensions;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("projects")]
public class ProjectsController(IProjectsService projectsService) : Controller
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectDTO>>> GetListAsync()
    {
        var list = await projectsService.GetListAsync();
        var mappedList = list.Select(it => it.ToDTO());
        
        return Ok(mappedList);
    }
    
    [HttpPost]
    public Task<ActionResult> AddAsync(ProjectDTO projectDTO)
    {
        return this.CreatedWithDomainExceptionsHandling(
            () => projectsService.AddAsync(projectDTO.ToDomain())
        );
    }
}