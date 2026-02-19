using System.Reactive.Disposables;

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
    private readonly ITemplate                 m_template;

    public TemplateSaveFormViewModel(IAbstractTemplatesService templatesService, ITemplate template)
    {
        m_templatesService = templatesService;
        m_template = template;

        ReloadCommand = ReactiveCommand.CreateFromTask(ReloadAsync);
    }

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);

        ReloadCommand.Execute().Subscribe().DisposeWith(disposables);
    }

    public string TemplateName { get; set; } = string.Empty;

    [Reactive]
    public IReadOnlyList<ITemplateGroup> TemplateGroups { get; private set; } = [];

    [Reactive]
    public IReadOnlyList<ITemplate> UngrouppedTemplates { get; private set; } = [];

    public ReactiveCommand<Unit, Unit> ReloadCommand { get; }

    public async Task SaveTemplateAsync()
    {
        if(string.IsNullOrWhiteSpace(TemplateName)) return;

        m_template.TemplateName = TemplateName;
        await m_templatesService.AddAsync(m_template, string.Empty);
        Close(true);
    }

    private async Task ReloadAsync()
    {
        var groups = await m_templatesService.GetAbstractTemplatesListAsync();

        TemplateGroups = groups.Where(it => !string.IsNullOrEmpty(it.Name)).ToArray();

        UngrouppedTemplates = groups.FirstOrDefault(it => string.IsNullOrEmpty(it.Name))?.Templates ?? [];
    }
}

public class TemplateSaveFormViewModel<TTemplate>(ITemplatesService<TTemplate> templatesService, TTemplate template)
    : TemplateSaveFormViewModel(templatesService, template) where TTemplate : ITemplate;