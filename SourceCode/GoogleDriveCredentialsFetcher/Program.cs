// See https://aka.ms/new-console-template for more information

using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Util.Store;

Console.WriteLine("Путь до secrets.json");
var secretsPath = Console.ReadLine();
Console.WriteLine("Путь до file store");
var fileStorePath = Console.ReadLine();
if (string.IsNullOrEmpty(fileStorePath)) fileStorePath = Environment.CurrentDirectory;
Console.WriteLine("Пользователь");
var user = Console.ReadLine();

await GoogleWebAuthorizationBroker.AuthorizeAsync(
    GoogleClientSecrets.FromFile(secretsPath).Secrets,
    new[] { DriveService.Scope.Drive },
    user,
    CancellationToken.None,
    new FileDataStore(fileStorePath)
);

Console.WriteLine("Token saved to " + fileStorePath);