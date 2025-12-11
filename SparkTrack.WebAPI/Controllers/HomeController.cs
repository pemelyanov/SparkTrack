using Microsoft.AspNetCore.Mvc;

namespace SparkTrack.WebAPI.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return Ok();
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