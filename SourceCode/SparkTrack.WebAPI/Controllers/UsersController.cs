namespace SparkTrack.WebAPI.Controllers;

using Core.Services.Users;
using Core.Shared.Enums;
using DTO;
using DTO.Edit;
using Extensions;
using MappingExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("users")]
public class UsersController(IUsersService usersService) : Controller
{
    [Authorize(Roles = $"{nameof(ERole.God)}, {nameof(ERole.Admin)}")]
    [HttpGet("admins")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedDTO<UserDTO>>> GetAdminsList([FromQuery] PageQueryDTO pageQuery)
    {
        var page = await usersService.GetPageAsync(ERole.Admin, pageQuery.ToDomain());

        return Ok(page.ToDTO(it => it.ToDTO()));
    }

    [Authorize(Roles = $"{nameof(ERole.God)}, {nameof(ERole.Admin)}")]
    [HttpGet("employees")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedDTO<UserDTO>>> GetEmployeesList([FromQuery] PageQueryDTO pageQuery)
    {
        var page = await usersService.GetPageAsync(ERole.Employee, pageQuery.ToDomain());

        return Ok(page.ToDTO(it => it.ToDTO()));
    }
    
    [Authorize(Roles = $"{nameof(ERole.God)}, {nameof(ERole.Admin)}")]
    [HttpPatch("users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult> EditAsync(UserEditDTO userEditDTO)
    {
        return this.OkWithDomainExceptionsHandling(() => usersService.EditAsync(userEditDTO.ToDomain()));
    }
    
    [Authorize(Roles = $"{nameof(ERole.God)}, {nameof(ERole.Admin)}")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<ActionResult> DeleteAsync([FromRoute]Guid id, [FromQuery] bool force)
    {
        return this.OkWithDomainExceptionsHandling(() => usersService.DeleteAsync(id, force));
    }
}