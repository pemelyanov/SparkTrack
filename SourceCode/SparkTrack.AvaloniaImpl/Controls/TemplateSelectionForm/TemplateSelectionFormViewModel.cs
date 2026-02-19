using System.Reactive;
using System.Reactive.Disposables;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SparkTrack.AvaloniaImpl.Controls.TemplateSaveForm;
using SparkTrack.AvaloniaImpl.Data.Templates;
using SparkTrack.AvaloniaImpl.Services.DialogHost;
using SparkTrack.AvaloniaImpl.Services.Templates;
using SparkTrack.AvaloniaImpl.ViewModels;

namespace SparkTrack.AvaloniaImpl.Controls.TemplateSelectionForm;

public class TemplateSelectionFormViewModel : DialogViewModelBase
{
    private readonly IAbstractTemplatesService m_templatesService;

    public TemplateSelectionFormViewModel(IAbstractTemplatesService templatesService)
    {
        m_templatesService = templatesService;

        ReloadCommand = ReactiveCommand.CreateFromTask(ReloadAsync);

        RegisterPropertyChangedHandler<TemplateSelectionFormViewModel>(it => it.SelectedGroup,
            OnSelectedGroupChanged)
            .DisposeWith(m_disposables);
        
        RegisterPropertyChangedHandler<TemplateSelectionFormViewModel>(it => it.SelectedTemplate,
            OnSelectedTemplateChanged)
            .DisposeWith(m_disposables);
    }

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);

        ReloadCommand.Execute().Subscribe().DisposeWith(disposables);
    }

    [Reactive]
    public IReadOnlyList<TemplateTreeItemProxy> TemplateGroups { get; private set; } = [];

    [Reactive]
    public IReadOnlyList<ITemplate> UngrouppedTemplates { get; private set; } = [];
    
    [Reactive]
    public TemplateTreeItemProxy? SelectedGroup { get; set; }
    
    [Reactive]
    public ITemplate? SelectedTemplate { get; set; }

    public ReactiveCommand<Unit, Unit> ReloadCommand { get; }

    private async Task ReloadAsync()
    {
        var groups = await m_templatesService.GetAbstractTemplatesListAsync();

        TemplateGroups = groups.Where(it => !string.IsNullOrEmpty(it.Name)).Select(it => new TemplateTreeItemProxy(it))
            .ToArray();

        UngrouppedTemplates = groups.FirstOrDefault(it => string.IsNullOrEmpty(it.Name))?.Templates ?? [];
    }
    
    private void OnSelectedGroupChanged()
    {
        if (SelectedGroup?.Template is { } template)
        {
            SelectedTemplate = template;
        }
        else
        {
            SelectedTemplate = null;
        }
    }
    
    private void OnSelectedTemplateChanged()
    {
        SelectedGroup = null;
    }
}

public class TemplateSelectionFormViewModel<TTemplate>(ITemplatesService<TTemplate> templatesService)
    : TemplateSelectionFormViewModel(templatesService) where TTemplate : ITemplate;