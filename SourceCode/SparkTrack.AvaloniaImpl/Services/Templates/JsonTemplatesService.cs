using System.Text.Json;
using SparkTrack.AvaloniaImpl.Data.Templates;

namespace SparkTrack.AvaloniaImpl.Services.Templates;

public class JsonTemplatesService<TTemplate>(string templateCategoryName)
    : ITemplatesService<TTemplate> where TTemplate : ITemplate
{
    private const string UngroupedFolderName = "Ungrouped";
    
    private readonly JsonSerializerOptions m_serializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string m_rootPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SparkTrack", "Templates");

    public Task<IReadOnlyList<TemplateGroup<TTemplate>>> GetTemplatesListAsync()
    {
        if (!Path.Exists(m_rootPath)) return Task.FromResult<IReadOnlyList<TemplateGroup<TTemplate>>>([]);
        
        var groupPaths = Directory.GetDirectories(m_rootPath);

        IReadOnlyList<TemplateGroup<TTemplate>> groups = groupPaths
            .Select(it => new DirectoryInfo(it))
            .Select(group =>
            {
                var templatesPath = Path.Combine(group.FullName, templateCategoryName);
                var groupName = group.Name == UngroupedFolderName ? string.Empty : group.Name;

                if (!Path.Exists(templatesPath)) return new TemplateGroup<TTemplate>
                {
                    Name = groupName
                };
                
                var templates = Directory.GetFiles(templatesPath)
                    .Select(templatePath => JsonSerializer.Deserialize<TTemplate>(File.ReadAllText(templatePath), m_serializerOptions))
                    .Where(it => it is not null)
                    .Select(it => it!);

                return new TemplateGroup<TTemplate>
                {
                    Name = groupName,
                    Templates = templates.ToArray()
                };
            })
            .ToArray();

        return Task.FromResult(groups);
    }

    public async Task<IReadOnlyList<ITemplateGroup>> GetAbstractTemplatesListAsync() => await GetTemplatesListAsync();

    public Task AddAsync(ITemplate template, string? group = null)
    {
        if (template is not TTemplate typedTemplate)
            throw new ArgumentException($"Unsupported type. Template must be {typeof(TTemplate)}");

        return AddAsync(typedTemplate, group);
    }

    public Task RemoveAsync(ITemplate template, string? group = null)
    {
        if (template is not TTemplate typedTemplate)
            throw new ArgumentException($"Unsupported type. Template must be {typeof(TTemplate)}");

        return RemoveAsync(typedTemplate, group);
    }

    public Task AddAsync(TTemplate template, string? group = null)
    {
        var data = JsonSerializer.Serialize(template, m_serializerOptions);

        group = string.IsNullOrWhiteSpace(group) ? UngroupedFolderName : group;

        var templatePath = EnsureTemplatePath(group, template);

        File.WriteAllText(templatePath, data);

        return Task.CompletedTask;
    }

    public Task RemoveAsync(TTemplate template, string? group = null)
    {
        group = string.IsNullOrWhiteSpace(group) ? UngroupedFolderName : group;
        
        var templatePath = EnsureTemplatePath(group, template);

        if (File.Exists(templatePath)) File.Delete(templatePath);

        return Task.CompletedTask;
    }

    public Task AddGroupAsync(string name)
    {
        if (string.IsNullOrEmpty(name)) return Task.CompletedTask;
        
        EnsureGroupFolderPath(name);

        return Task.CompletedTask;
    }

    public Task RemoveGroupAsync(string name)
    {
        if (string.IsNullOrEmpty(name) || name == UngroupedFolderName) return Task.CompletedTask;
        
        var groupFolder = EnsureGroupFolderPath(name);

        Directory.Delete(groupFolder, true);

        return Task.CompletedTask;
    }

    private string EnsureGroupFolderPath(string groupName)
    {
        var path = Path.Combine(m_rootPath, groupName);

        Directory.CreateDirectory(path);

        return path;
    }

    private string EnsureTemplatePath(string groupName, TTemplate template)
    {
        var folder = Path.Combine(m_rootPath, groupName, templateCategoryName);
        var templatePath = Path.Combine(folder, template.TemplateName + ".json");

        Directory.CreateDirectory(folder);

        return templatePath;
    }
}