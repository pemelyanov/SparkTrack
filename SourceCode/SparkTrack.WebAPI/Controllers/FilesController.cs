namespace SparkTrack.WebAPI.Controllers;

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
    public async Task<ActionResult<Guid>> UploadAsync(IFormFile file)
    {
        await using var fileStream = file.OpenReadStream();

        var fileId = await filesService.UploadAsync(fileStream);

        return Ok(fileId);
    }
    
    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces("application/octet-stream")]
    public async Task<IActionResult> DownloadAsync([FromRoute] Guid id)
    {
        await using var fileStream = await filesService.DownloadAsync(id);

        if (fileStream is null) return NotFound();

        return File(fileStream, "application/octet-stream");
    }
}