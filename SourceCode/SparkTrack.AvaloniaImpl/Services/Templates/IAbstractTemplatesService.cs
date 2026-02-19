using SparkTrack.AvaloniaImpl.Data.Templates;

namespace SparkTrack.AvaloniaImpl.Services.Templates;

public interface ITemplatesService<TTemplate> : IAbstractTemplatesService where TTemplate : ITemplate
{
    Task<IReadOnlyList<TemplateGroup<TTemplate>>> GetTemplatesListAsync();

    Task AddAsync(TTemplate template, string group);

    Task RemoveAsync(TTemplate template, string group);
}

public interface IAbstractTemplatesService : ITemplateGroupsService
{
    Task<IReadOnlyList<ITemplateGroup>> GetAbstractTemplatesListAsync();
    
    Task AddAsync(ITemplate template, string group);

    Task RemoveAsync(ITemplate template, string group);
}