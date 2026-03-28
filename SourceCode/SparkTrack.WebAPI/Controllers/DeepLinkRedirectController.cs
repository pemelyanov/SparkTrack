namespace SparkTrack.WebAPI.Controllers;

using DeepLink;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("open-app")]
public class DeepLinkRedirectController : Controller
{
    public IActionResult RedirectToDeepLink()
    {
        var rawQueryString = HttpContext.Request.QueryString.Value;

        var deepLink = SparkTrackDeepLink.FromQuery(rawQueryString);
        
        return Redirect(deepLink);
    }
}