namespace SparkTrack.Core.Client.Services.Accounts;

using Data;

public interface IAccountsService
{
    Task<IReadOnlyList<Account>> GetAccountsListAsync();

    Task SaveAccountAsync(Account account);

    Task RemoveAccountAsync(string email);

    Task UseAccountAsync(Account account);
}