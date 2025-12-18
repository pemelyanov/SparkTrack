namespace SparkTrack.WebAPI.Controllers;

using Core.Shared.Enums;
using DTO;
using DTO.Edit;
using Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.JwtAuthorization;

[ApiController]
[Route("authorization")]
public class AuthorizationController(IJwtAuthorizationService authorizationService) : Controller
{
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuthorizationDTO>?> LogInAsync(LogInDTO logInDTO)
    {
        var dto = await authorizationService.LogInAsync(logInDTO.Email, logInDTO.Password);

        if (dto is null) return NotFound();

        return Ok(dto);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuthorizationDTO?>> RefreshTokensAsync(TokenRefreshDTO tokenRefreshDTO)
    {
        var dto = await authorizationService.RefreshTokensAsync(tokenRefreshDTO.RefreshToken);

        if (dto is null) return NotFound();

        return Ok(dto);
    }

    [HttpDelete("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> LogOutAsync(string refreshToken)
    {
        await authorizationService.LogOutAsync(refreshToken);

        return Ok();
    }

    [HttpDelete("logout-all")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> LogOutAllAsync(Guid userId)
    {
        await authorizationService.LogOutAllAsync(userId);

        return Ok();
    }

    [HttpPost("register/admin")]
    [Authorize(Roles = nameof(ERole.God))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<ActionResult<string>> RegisterAdminAsync(UserEditDTO userEdit)
    {
        return this.OkWithDomainExceptionsHandling(() => authorizationService.RegisterAsync(userEdit, ERole.Admin));
    }

    [HttpPost("register/employee")]
    [Authorize(Roles = nameof(ERole.Admin))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<ActionResult<string>> RegisterEmployeeAsync(UserEditDTO userEdit)
    {
        return this.OkWithDomainExceptionsHandling(() => authorizationService.RegisterAsync(userEdit, ERole.Employee));
    }

    [HttpPatch("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> ChangePassword(ChangePasswordDTO changePasswordDTO)
    {
        var result = await authorizationService.ChangePassword(
            changePasswordDTO.OldPassword,
            changePasswordDTO.NewPassword
        );

        return result ? Ok() : BadRequest();
    }
}