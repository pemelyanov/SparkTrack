namespace SparkTrack.WebAPI.Controllers;

using Core.Shared.Enums;
using Core.Shared.Services.SubTasks;
using DTO;
using Extensions;
using MappingExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("subtasks")]
public class SubTasksController(ISubTasksService subTasksService) : Controller
{
    [HttpPatch("{id}/is-completed")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<ActionResult<SubTaskDTO>> SetIsCompletedAsync([FromRoute] Guid id, bool value, Guid currentVersion)
    {
        return this.OkWithDomainExceptionsHandling(
            async () =>
            {
                var subTask = await subTasksService.SetIsCompletedAsync(id, value, currentVersion);

                return subTask?.ToDTO()!;
            }
        );
    }
    
    [HttpPatch("{id}/payment-status")]
    [Authorize(Roles = nameof(ERole.Admin))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<ActionResult<SubTaskDTO>> SetPaymentStatusAsync([FromRoute] Guid id, EPaymentStatus value, Guid currentVersion)
    {
        return this.OkWithDomainExceptionsHandling(
            async () =>
            {
                var subTask = await subTasksService.SetPaymentStatusAsync(id, value, currentVersion);

                return subTask?.ToDTO()!;
            }
        );
    }
}