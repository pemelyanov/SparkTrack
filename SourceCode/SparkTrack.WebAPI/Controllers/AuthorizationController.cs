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
    [HttpPost]
    public async Task<ActionResult<AuthorizationDTO>?> LogInAsync(LogInDTO logInDTO)
    {
        var dto = await authorizationService.LogInAsync(logInDTO.Email, logInDTO.Password);

        return Ok(dto);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<AuthorizationDTO?>> RefreshTokensAsync(TokenRefreshDTO tokenRefreshDTO)
    {
        var dto = await authorizationService.RefreshTokensAsync(tokenRefreshDTO.RefreshToken);

        return Ok(dto);
    }

    [HttpDelete]
    [Authorize]
    public async Task<ActionResult> LogOutAsync(string refreshToken)
    {
        await authorizationService.LogOutAsync(refreshToken);

        return Ok();
    }

    [HttpDelete]
    [Authorize]
    public async Task<ActionResult> LogOutAllAsync(Guid userId)
    {
        await authorizationService.LogOutAllAsync(userId);

        return Ok();
    }

    [HttpPost]
    // TODO: как-то подвязать enum
    [Authorize(Roles = "God")]
    public async Task<ActionResult<string>> RegisterAdminAsync(UserEditDTO userEdit)
    {
        var password = await authorizationService.RegisterAsync(userEdit, ERole.Admin);

        return Ok(password);
    }

    [HttpPost]
    // TODO: как-то подвязать enum
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<string>> RegisterEmployeeAsync(UserEditDTO userEdit)
    {
        var password = await authorizationService.RegisterAsync(userEdit, ERole.Employee);

        return Ok(password);
    }

    [HttpPatch]
    [Authorize]
    public async Task<ActionResult> ChangePassword(ChangePasswordDTO changePasswordDTO) // TODO: переделать на DTO
    {
        var result = await authorizationService.ChangePassword(
            changePasswordDTO.OldPassword,
            changePasswordDTO.NewPassword
        );

        return result ? Ok() : BadRequest();
    }
}