namespace SparkTrack.WebAPI.Controllers;

using Core.Services.Users;
using Core.Shared.Enums;
using DTO;
using MappingExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("users")]
public class UsersController(IUsersService usersService) : Controller
{
    [Authorize(Roles = nameof(ERole.God))]
    [HttpGet("admins")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedDTO<UserDTO>>> GetAdminsList([FromQuery] PageQueryDTO pageQuery)
    {
        var page = await usersService.GetPageAsync(ERole.Admin, pageQuery.ToDomain());

        return Ok(page.ToDTO(it => it.ToDTO()));
    }

    [Authorize(Roles = nameof(ERole.God))]
    [HttpGet("employees")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedDTO<UserDTO>>> GetEmployeesList([FromQuery] PageQueryDTO pageQuery)
    {
        var page = await usersService.GetPageAsync(ERole.Employee, pageQuery.ToDomain());

        return Ok(page.ToDTO(it => it.ToDTO()));
    }
}