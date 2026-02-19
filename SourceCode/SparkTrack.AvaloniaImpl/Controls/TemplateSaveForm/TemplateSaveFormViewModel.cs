namespace SparkTrack.AvaloniaImpl.Controls.TemplateSaveForm;

using System.Reactive;
using Data.Templates;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Services.Templates;
using ViewModels;

public class TemplateSaveFormViewModel : DialogViewModelBase
{
    private readonly IAbstractTemplatesService m_templatesService;

    public TemplateSaveFormViewModel(IAbstractTemplatesService templatesService)
    {
        m_templatesService = templatesService;

        ReloadCommand = ReactiveCommand.CreateFromTask(ReloadAsync);
    }

    public string TemplateName { get; set; } = string.Empty;

    [Reactive]
    public IReadOnlyList<ITemplateGroup> TemplateGroups { get; private set; } = [];

    [Reactive]
    public IReadOnlyList<ITemplate> UngrouppedTemplates { get; private set; } = [];

    public ReactiveCommand<Unit, Unit> ReloadCommand { get; }

    private async Task ReloadAsync()
    {
        var groups = await m_templatesService.GetAbstractTemplatesListAsync();

        TemplateGroups = groups.Where(it => !string.IsNullOrEmpty(it.Name)).ToArray();

        UngrouppedTemplates = groups.FirstOrDefault(it => string.IsNullOrEmpty(it.Name))?.Templates ?? [];
    }
}

public class TemplateSaveFormViewModel<TTemplate>(ITemplatesService<TTemplate> templatesService)
    : TemplateSaveFormViewModel(templatesService) where TTemplate : ITemplate;