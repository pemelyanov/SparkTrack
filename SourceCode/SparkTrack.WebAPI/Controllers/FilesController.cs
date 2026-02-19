namespace SparkTrack.WebAPI.Controllers;

using ActionResults;
using Core.Services.Files;
using Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("files")]
public class FilesController(IFilesService filesService) : Controller
{
    [Authorize]
    [HttpPost]
    [DisableRequestSizeLimit]
    [DisableRequestTimeout]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public Task<ActionResult<Guid>> UploadAsync(CancellationToken cancellationToken, [FromHeader(Name = "Content-Length")] long contentLength)
    {
        return this.OkWithDomainExceptionsHandling(() => filesService.UploadAsync(Request.Body, contentLength, cancellationToken));
    }

    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces("application/octet-stream")]
    public IActionResult DownloadAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = new PushStreamResult(
            "application/octet-stream",
            id.ToString(),
            (stream, contentLengthCallback) => filesService.DownloadAsync(
                id,
                stream,
                cancellationToken,
                contentLengthCallback
            )
        );

        return result;
    }
}