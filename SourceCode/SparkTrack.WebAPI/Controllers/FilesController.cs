namespace SparkTrack.WebAPI.Controllers;

using ActionResults;
using Core.Services.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("files")]
public class FilesController(IFilesService filesService) : ControllerBase
{
    [Authorize]
    [HttpPost]
    [DisableRequestSizeLimit]
    [DisableRequestTimeout]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Guid>> UploadAsync(CancellationToken cancellationToken)
    {
        var fileId = await filesService.UploadAsync(Request.Body, cancellationToken);

        return Ok(fileId);
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