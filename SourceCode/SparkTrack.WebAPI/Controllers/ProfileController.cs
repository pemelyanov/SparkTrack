namespace SparkTrack.WebAPI.Controllers;

using DTO;
using MappingExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IAuthorizationService = Core.Services.Authorization.IAuthorizationService;

[ApiController]
[Route("profile")]
public class ProfileController(IAuthorizationService authorizationService) : Controller
{
    [Authorize]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<UserDTO> Get()
    {
        return Ok(authorizationService.CurrentUser!.ToDTO());
    }
}