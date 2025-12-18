namespace SparkTrack.API.Interceptors;

using System.Net;
using System.Net.Http.Headers;
using Core.Client.Services.Configuration;
using Data;
using NLog;

public class RetryAuthHandler(
    IConfigurationService<TokensConfiguration> tokensConfiguration,
    Func<ClientWrapper<AuthorizationClient>> authorizationClientFactory
) : DelegatingHandler(new HttpClientHandler())
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var token = tokensConfiguration.Config.AccessToken;
        request.Headers.Authorization = GetAuthenticationHeader(token);

        var response = await base.SendAsync(request, cancellationToken);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            s_logger.Info("Refreshing token...");

            var refreshToken = tokensConfiguration.Config.RefreshToken;

            var authorizationClientWrapper = authorizationClientFactory.Invoke();

            var tokensDTO = await authorizationClientWrapper.Client.RefreshTokensAsync(
                new TokenRefreshDTO
                {
                    RefreshToken = refreshToken
                },
                cancellationToken
            );
            
            request.Headers.Authorization = GetAuthenticationHeader(tokensDTO.AccessToken);
            
            response = await base.SendAsync(request, cancellationToken);

            tokensConfiguration.UpdateConfig(
                new TokensConfiguration
                {
                    AccessToken = tokensDTO.AccessToken,
                    RefreshToken = tokensDTO.RefreshToken
                }
            );
        }

        return response;
    }

    private static AuthenticationHeaderValue GetAuthenticationHeader(string token) => new("Bearer", token);
}