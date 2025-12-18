namespace SparkTrack.WebAPI.Controllers;

using Core.Shared.Enums;
using DTO;
using DTO.Edit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.JwtAuthorization;

[ApiController]
[Route("authorization")]
public class AuthorizationController(IJwtAuthorizationService authorizationService) : Controller
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthorizationDTO>?> LogInAsync(LogInDTO logInDTO)
    {
        var dto = await authorizationService.LogInAsync(logInDTO.Email, logInDTO.Password);

        return Ok(dto);
    }

    [HttpPost("refresh")]
    [Authorize]
    public async Task<ActionResult<AuthorizationDTO?>> RefreshTokensAsync(TokenRefreshDTO tokenRefreshDTO)
    {
        var dto = await authorizationService.RefreshTokensAsync(tokenRefreshDTO.RefreshToken);

        return Ok(dto);
    }

    [HttpDelete("logout")]
    [Authorize]
    public async Task<ActionResult> LogOutAsync(string refreshToken)
    {
        await authorizationService.LogOutAsync(refreshToken);

        return Ok();
    }

    [HttpDelete("logout-all")]
    [Authorize]
    public async Task<ActionResult> LogOutAllAsync(Guid userId)
    {
        await authorizationService.LogOutAllAsync(userId);

        return Ok();
    }

    [HttpPost("register/admin")]
    [Authorize(Roles = nameof(ERole.God))]
    public async Task<ActionResult<string>> RegisterAdminAsync(UserEditDTO userEdit)
    {
        var password = await authorizationService.RegisterAsync(userEdit, ERole.Admin);

        return Ok(password);
    }

    [HttpPost("register/employee")]
    [Authorize(Roles = nameof(ERole.Admin))]
    public async Task<ActionResult<string>> RegisterEmployeeAsync(UserEditDTO userEdit)
    {
        var password = await authorizationService.RegisterAsync(userEdit, ERole.Employee);

        return Ok(password);
    }

    [HttpPatch("change-password")]
    [Authorize]
    public async Task<ActionResult> ChangePassword(ChangePasswordDTO changePasswordDTO)
    {
        var result = await authorizationService.ChangePassword(
            changePasswordDTO.OldPassword,
            changePasswordDTO.NewPassword
        );

        return result ? Ok() : BadRequest();
    }
}