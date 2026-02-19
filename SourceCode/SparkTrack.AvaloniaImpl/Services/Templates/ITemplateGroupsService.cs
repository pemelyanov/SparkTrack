namespace SparkTrack.AvaloniaImpl.Services.Templates;

public interface ITemplateGroupsService
{
    Task AddGroupAsync(string name);

    Task RemoveGroupAsync(string name);
}