using System.Text.Json;
using SparkTrack.AvaloniaImpl.Data.Templates;

namespace SparkTrack.AvaloniaImpl.Services.Templates;

public class JsonTemplatesService<TTemplate>(string templateCategoryName)
    : ITemplatesService<TTemplate> where TTemplate : ITemplate
{
    private readonly JsonSerializerOptions m_serializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string m_rootPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SpartTrack", "Templates");

    public Task<IReadOnlyList<TemplateGroup<TTemplate>>> GetTemplatesListAsync()
    {
        var groupPaths = Directory.GetDirectories(m_rootPath);

        IReadOnlyList<TemplateGroup<TTemplate>> groups = groupPaths.Where(groupPath => Directory
                .GetDirectories(groupPath).Select(it => new DirectoryInfo(it))
                .Any(it => it.Name == templateCategoryName))
            .Select(group =>
            {
                var templates = Directory.GetFiles(Path.Combine(group, templateCategoryName))
                    .Select(templatePath => JsonSerializer.Deserialize<TTemplate>(templatePath, m_serializerOptions))
                    .Where(it => it is not null)
                    .Select(it => it!);

                return new TemplateGroup<TTemplate>
                {
                    Name = new DirectoryInfo(group).Name,
                    Templates = templates.ToArray()
                };
            }).ToArray();

        return Task.FromResult(groups);
    }

    public Task AddAsync(TTemplate template, string group)
    {
        var data = JsonSerializer.Serialize(template, m_serializerOptions);

        var templatePath = EnsureTemplatePath(group, template);

        File.WriteAllText(templatePath, data);

        return Task.CompletedTask;
    }

    public Task RemoveAsync(TTemplate template, string group)
    {
        var templatePath = EnsureTemplatePath(group, template);

        if (File.Exists(templatePath)) File.Delete(templatePath);

        return Task.CompletedTask;
    }

    public Task AddGroupAsync(string name)
    {
        EnsureGroupFolderPath(name);

        return Task.CompletedTask;
    }

    public Task RemoveGroupAsync(string name)
    {
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