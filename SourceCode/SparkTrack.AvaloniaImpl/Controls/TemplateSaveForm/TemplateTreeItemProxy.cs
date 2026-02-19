using SparkTrack.AvaloniaImpl.Data.Templates;

namespace SparkTrack.AvaloniaImpl.Controls.TemplateSaveForm;

public class TemplateTreeItemProxy
{
    public TemplateTreeItemProxy(ITemplate template)
    {
        Template = template;
        Name = Template.TemplateName;
    }

    public TemplateTreeItemProxy(ITemplateGroup group)
    {
        Group = group;
        Name = Group.Name;
        Children = group.Templates.Select(it => new TemplateTreeItemProxy(it)).ToArray();
    }
    
    public ITemplate? Template { get; }
    
    public ITemplateGroup? Group { get; }

    public string Name { get; }

    public IReadOnlyList<TemplateTreeItemProxy> Children { get; } = [];
}