namespace SparkTrack.AvaloniaImpl.Controls.TemplatesList;

using Data.Templates;

public class TemplateTreeItemProxy
{
    public TemplateTreeItemProxy(ITemplate template, TemplateTreeItemProxy parent)
    {
        Parent = parent;
        Template = template;
        Name = Template.TemplateName;
    }

    public TemplateTreeItemProxy(ITemplateGroup group)
    {
        Group = group;
        Name = Group.Name;
        Children = group.Templates.Select(it => new TemplateTreeItemProxy(it, this)).ToArray();
    }
    
    public ITemplate? Template { get; }
    
    public ITemplateGroup? Group { get; }

    public string Name { get; }
    
    public TemplateTreeItemProxy? Parent { get; }

    public IReadOnlyList<TemplateTreeItemProxy> Children { get; } = [];
}