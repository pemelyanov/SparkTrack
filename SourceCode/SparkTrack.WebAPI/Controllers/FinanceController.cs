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
        return this.OkWithDomainExceptionsHandling(async () =>
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
    public Task<ActionResult<IReadOnlyList<UserPaymentDTO>>> GetUsersRemainingPaymentsAsync(Guid? projectId)
    {
        return this.OkWithDomainExceptionsHandling(async () =>
            {
                var data = await paymentBillsService.GetUsersRemainingPaymentsAsync(projectId);

                return data.Select(it => it.ToDTO()).ToArray() as IReadOnlyList<UserPaymentDTO>;
            }
        );
    }

    [HttpGet("pending-payments-summary")]
    [Authorize(Roles = nameof(ERole.Admin))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<ActionResult<PendingPaymentsSummaryDTO>> GetPendingPaymentsSummaryAsync(Guid? projectId)
    {
        return this.OkWithDomainExceptionsHandling(async () =>
            {
                var data = await paymentBillsService.GetPendingPaymentsSummaryAsync(projectId);

                return data.ToDTO();
            }
        );
    }

    [HttpGet("payments-history")]
    [Authorize(Roles = nameof(ERole.Admin))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<ActionResult<PagedDTO<PaymentDetailsDTO>>> GetAdminPaymentsHistoryAsync(
        Guid? adminId,
        Guid? employeeId,
        Guid? projectId,
        DateTime? startDate,
        DateTime? endDate,
        [FromQuery] PageQueryDTO pageQuery
    )
    {
        return this.OkWithDomainExceptionsHandling(async () =>
            {
                var data = await paymentBillsService.GetPaidPaymentsListAsync(
                    adminId,
                    employeeId,
                    projectId,
                    startDate?.ToUniversalTime(),
                    endDate?.ToUniversalTime(),
                    pageQuery.ToDomain()
                );

                return data.ToDTO(it => it.ToDTO());
            }
        );
    }

    [HttpGet("bonus-history")]
    [Authorize(Roles = nameof(ERole.Admin))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<ActionResult<PagedDTO<BonusPaymentDTO>>> GetAdminBonusPaymentsHistoryAsync(
        Guid? adminId,
        Guid? employeeId,
        DateTime? startDate,
        DateTime? endDate,
        [FromQuery] PageQueryDTO pageQuery
    )
    {
        return this.OkWithDomainExceptionsHandling(async () =>
            {
                var data = await paymentBillsService.GetPaidBonusPaymentsListAsync(
                    adminId,
                    employeeId,
                    startDate,
                    endDate,
                    pageQuery.ToDomain()
                );

                return data.ToDTO(it => it.ToDTO());
            }
        );
    }

    [HttpPut("bills")]
    [Authorize(Roles = nameof(ERole.Admin))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<ActionResult> PayBillsAsync(
        [FromQuery] IReadOnlyList<Guid> tasksIdList,
        float payment,
        float timelyBonusPayment
    )
    {
        return this.OkWithDomainExceptionsHandling(() =>
            paymentBillsService.PayBillsAsync(tasksIdList, payment, timelyBonusPayment)
        );
    }

    [HttpPut("bonus")]
    [Authorize(Roles = nameof(ERole.Admin))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<ActionResult> PayBonusAsync(Guid employeeId, float payment, string? comment)
    {
        return this.OkWithDomainExceptionsHandling(() => paymentBillsService.PayBonusAsync(employeeId, payment, comment)
        );
    }

    [HttpDelete("bill/{id}")]
    [Authorize(Roles = nameof(ERole.Admin))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<ActionResult> DeleteBillAsync(Guid id)
    {
        return this.OkWithDomainExceptionsHandling(() => paymentBillsService.DeleteBillAsync(id)
        );
    }

    [HttpGet("bonus/{id}")]
    [Authorize(Roles = nameof(ERole.Admin))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<ActionResult> DeleteBonusAsync(Guid id)
    {
        return this.OkWithDomainExceptionsHandling(() => paymentBillsService.DeleteBonusAsync(id)
        );
    }
}