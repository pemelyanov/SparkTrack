namespace SparkTrack.WebAPI.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("open-app")]
public class DeepLinkRedirectController : Controller
{
    public IActionResult RedirectToDeepLink()
    {
        var rawQueryString = HttpContext.Request.QueryString.Value;
        string deeplink = $"sparktrack://{rawQueryString}";
        
        return Redirect(deeplink);
    }
}