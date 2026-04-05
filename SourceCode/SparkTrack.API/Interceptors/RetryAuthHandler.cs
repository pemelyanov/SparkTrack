namespace SparkTrack.API.Interceptors;

using System.Net;
using System.Net.Http.Headers;
using Core.Client.Data;
using Core.Client.Services.Configuration;
using NLog;

public class RetryAuthHandler(
    IConfigurationService<TokensConfiguration> tokensConfiguration,
    Func<ClientWrapper<AuthorizationClient>> authorizationClientFactory
) : DelegatingHandler(new HttpClientHandler())
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();

    private static readonly SemaphoreSlim s_refreshLock = new(1, 1);

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        request.Headers.Authorization =
            GetAuthenticationHeader(tokensConfiguration.Config.AccessToken);

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        s_logger.Info("Unauthorized. Trying to refresh token...");

        await s_refreshLock.WaitAsync(cancellationToken);
        try
        {
            // повторно проверяем токен, возможно, другой поток уже обновил его
            request.Headers.Authorization =
                GetAuthenticationHeader(tokensConfiguration.Config.AccessToken);

            response = await base.SendAsync(request, cancellationToken);
            if (response.StatusCode != HttpStatusCode.Unauthorized)
                return response;

            s_logger.Info("Refreshing token...");

            var authorizationClientWrapper = authorizationClientFactory.Invoke();

            var tokensDTO = await authorizationClientWrapper.Client.RefreshTokensAsync(
                new TokenRefreshDTO
                {
                    RefreshToken = tokensConfiguration.Config.RefreshToken
                },
                cancellationToken
            );

            tokensConfiguration.UpdateConfig(
                new TokensConfiguration
                {
                    AccessToken = tokensDTO.AccessToken,
                    RefreshToken = tokensDTO.RefreshToken
                }
            );
        }
        finally
        {
            s_refreshLock.Release();
        }

        request.Headers.Authorization =
            GetAuthenticationHeader(tokensConfiguration.Config.AccessToken);

        return await base.SendAsync(request, cancellationToken);
    }

    private static AuthenticationHeaderValue GetAuthenticationHeader(string token) =>
        new("Bearer", token);
}