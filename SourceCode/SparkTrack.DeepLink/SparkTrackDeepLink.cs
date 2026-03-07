namespace SparkTrack.DeepLink;

using System.Collections.Specialized;
using System.Web;
using Data;

public class SparkTrackDeepLink
{
    public const string FeaturePage = "feature";

    #if DEBUG
    private const string Scheme = "sparktrack-debug";
    #else
    private const string Scheme = "sparktrack";
    #endif

    private const string IdParam   = "id";
    private const string PageParam = "page";

    public PageData PageData { get; }

    private SparkTrackDeepLink(PageData pageData)
    {
        PageData = pageData;
    }

    public static string ToFeature(int featureId, string? customBaseUrl = null)
    {
        var queryParams = new NameValueCollection
        {
            [PageParam] = FeaturePage,
            [IdParam] = HttpUtility.HtmlEncode(featureId),
        };

        var baseUlr = string.IsNullOrEmpty(customBaseUrl) ? $"{Scheme}://" : customBaseUrl.TrimEnd("/");

        return $"{baseUlr}/?{ConstructQueryString(queryParams)}";
    }

    public static string FromQuery(string? query) => $"{Scheme}://?{query?.TrimStart('?')}";

    public static SparkTrackDeepLink Parse(string deepLink)
    {
        var uri = new Uri(deepLink);

        if (uri.Scheme != Scheme) throw new NotSupportedException($"Invalid scheme: {uri.Scheme}");
        var queryParams = HttpUtility.ParseQueryString(uri.Query);

        PageData data = queryParams[PageParam] switch
        {
            FeaturePage => new PageData.Feature(int.Parse(queryParams[IdParam]!)),
            _ => throw new NotSupportedException($"Unsupported page: {uri.Host}")
        };

        return new SparkTrackDeepLink(data);
    }

    private static string ConstructQueryString(NameValueCollection parameters)
    {
        if (parameters.Count == 0) return string.Empty;

        var items = parameters.AllKeys.Select(name => string.Concat(
                name,
                "=",
                HttpUtility.UrlEncode(parameters[name])
            )
        );

        return string.Join("&", items);
    }
}