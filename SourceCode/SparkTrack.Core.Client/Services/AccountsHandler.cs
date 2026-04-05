namespace SparkTrack.Core.Client.Services;

using Accounts;
using Authorization;
using Configuration;
using Data;
using Events;
using Shared.Eventing;

public class AccountsHandler(
    IAuthorizationService authorizationService,
    IAccountsService accountsService,
    IConfigurationService<TokensConfiguration> tokensConfiguration
) : IEventHandler<LogInEvent>
{
    public async Task HandleAsync(LogInEvent eventData, CancellationToken cancellationToken = default)
    {
        var currentUser = authorizationService.CurrentUser.Value;

        if (currentUser is null) return;

        var credentials = tokensConfiguration.Config;

        var account = new Account(currentUser.Name, currentUser.Email, currentUser.Role, credentials);

        await accountsService.SaveAccountAsync(account);
    }
}