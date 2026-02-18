using SparkTrack.AvaloniaImpl.Data.Templates;

namespace SparkTrack.AvaloniaImpl.Services.Templates;

public interface ITemplatesService<TTemplate> : ITemplateGroupsService where TTemplate : ITemplate
{
    Task<IReadOnlyList<TemplateGroup<TTemplate>>> GetTemplatesListAsync();

    Task AddAsync(TTemplate template, string group);

    Task RemoveAsync(TTemplate template, string group);
}