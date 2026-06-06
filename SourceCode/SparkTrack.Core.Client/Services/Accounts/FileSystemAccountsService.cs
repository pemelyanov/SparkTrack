namespace SparkTrack.Core.Client.Services.Accounts;

using System.Text.Json;
using Configuration;
using Data;
using NLog;

public class FileSystemAccountsService(IConfigurationService<TokensConfiguration> tokensConfiguration) : IAccountsService
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();
    
    private static readonly string s_accountsFolder = Path.Combine(Paths.ApplicationData, "Accounts");

    public Task<IReadOnlyList<Account>> GetAccountsListAsync()
    {
        if (!Directory.Exists(s_accountsFolder)) return Task.FromResult<IReadOnlyList<Account>>([]);

        var accounts = new List<Account>();

        var files = Directory.GetFiles(s_accountsFolder);

        foreach (var file in files)
        {
            try
            {
                var fileText = File.ReadAllText(file);
                var account = JsonSerializer.Deserialize<Account>(fileText);

                if (account is null)
                {
                    s_logger.Warn("Cannot parse account {path}", file);
                    continue;
                }
                
                accounts.Add(account);
            }
            catch (Exception e)
            {
                s_logger.Warn(e, "Cannot parse account");
            }
        }

        return Task.FromResult<IReadOnlyList<Account>>(accounts);
    }

    public Task SaveAccountAsync(Account account)
    {
        Directory.CreateDirectory(s_accountsFolder);

        string fileName = GetAccountFileName(account.Email);

        var accountString = JsonSerializer.Serialize(account);
        
        File.WriteAllText(Path.Combine(s_accountsFolder, fileName), accountString);

        return Task.CompletedTask;
    }

    public Task RemoveAccountAsync(string email)
    {
        if (!Directory.Exists(s_accountsFolder)) return Task.CompletedTask;

        var fileName = GetAccountFileName(email);
        
        File.Delete(Path.Combine(s_accountsFolder, fileName));
        
        return Task.CompletedTask;
    }

    public Task UseAccountAsync(Account account)
    {
        tokensConfiguration.UpdateConfig(account.Credentials);

        return Task.CompletedTask;
    }

    private static string GetAccountFileName(string email)
    {
        var accountFileName = Paths.NormalizeFileName(email);
        return accountFileName;
    }
}