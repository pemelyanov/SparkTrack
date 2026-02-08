namespace SparkTrack.WebAPI.ActionResults;

using System.Net;
using Microsoft.AspNetCore.Mvc;

public sealed class PushStreamResult(string contentType, string filename, Func<Stream, Action<long>, Task> callback)
    : FileResult(contentType)
{
    public override async Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;
        var encodedFilename = WebUtility.UrlEncode(filename);
        response.Headers["Content-Disposition"] = $"attachment; filename={encodedFilename}";
        await callback(
            response.BodyWriter.AsStream(),
            contentLength => response.Headers["Content-Length"] = contentLength.ToString()
        );
    }
}