namespace SparkTrack.WebAPI.Controllers;

using Core.Shared.Data;
using Core.Shared.Enums;
using Core.Shared.Services.PaymentBills;
using DTO;
using Extensions;
using MappingExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("finance")]
public class FinanceController(IPaymentBillsService paymentBillsService) : Controller
{
    [HttpGet("bills")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public Task<ActionResult<PagedDTO<PaymentBillDTO>>> GetBillsPageAsync(
        bool isPaid,
        Guid? projectId,
        [FromQuery] PageQuery pageQuery
    )
    {
        return this.OkWithDomainExceptionsHandling(
            async () =>
            {
                var page = await paymentBillsService.GetPageAsync(isPaid, projectId, pageQuery);

                return page.ToDTO(it => it.ToDTO());
            }
        );
    }

    [HttpGet("remaining-payments")]
    [Authorize(Roles = nameof(ERole.Admin))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<ActionResult<IReadOnlyList<UserRemainingPaymentDTO>>> GetUsersRemainingPaymentsAsync(Guid? projectId)
    {
        return this.OkWithDomainExceptionsHandling(
            async () =>
            {
                var data = await paymentBillsService.GetUsersRemainingPaymentsAsync(projectId);

                return data.Select(it => it.ToDTO()).ToArray() as IReadOnlyList<UserRemainingPaymentDTO>;
            }
        );
    }
}