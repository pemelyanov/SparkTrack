using Microsoft.AspNetCore.Mvc;

namespace SparkTrack.WebAPI.Controllers;

using Core.Services.Features;
using Core.Shared.Data;
using Core.Shared.Data.Edit;
using Core.Shared.Data.Entities;

public class HomeController(IFeaturesService featuresService) : Controller
{
    public async Task<ActionResult<IReadOnlyPagedData<Feature>>> Index()
    {
        await featuresService.AddAsync(
            new FeatureEdit
            {
                Name = "Test",
                ProjectId = Guid.Empty,
                TasksList = [],
                Deadline = DateTime.Now,
                Description = "asdasdasd",
                AttachmentsIdList = []
            }
        );
        
        return Ok(await featuresService.GetPageAsync(null, true, PageQuery.All));
    }

    public IActionResult Privacy()
    {
        return Ok();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return Ok();
    }
}