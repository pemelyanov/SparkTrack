using System.Reactive;
using System.Reactive.Disposables;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SparkTrack.AvaloniaImpl.Data.Templates;
using SparkTrack.AvaloniaImpl.Services.Templates;
using SparkTrack.AvaloniaImpl.ViewModels;

namespace SparkTrack.AvaloniaImpl.Controls.TemplateSelectionForm;

using TemplatesList;

public class TemplateSelectionFormViewModel : DialogViewModelBase
{
    private readonly IAbstractTemplatesService m_templatesService;
    
    public TemplateSelectionFormViewModel(IAbstractTemplatesService templatesService)
    {
        m_templatesService = templatesService;

        ReloadCommand = ReactiveCommand.CreateFromTask(ReloadAsync);
    }

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);

        ReloadCommand.Execute().Subscribe().DisposeWith(disposables);
    }

    [Reactive]
    public IReadOnlyList<TemplateTreeItemProxy> TemplateGroups { get; private set; } = [];

    [Reactive]
    public IReadOnlyList<ITemplate> UngroupedTemplates { get; private set; } = [];
    
    [Reactive]
    public ITemplate? SelectedTemplate { get; set; }

    public ReactiveCommand<Unit, Unit> ReloadCommand { get; }

    public void CloseWithTrue() => Close(true);

    private async Task ReloadAsync()
    {
        var groups = await m_templatesService.GetAbstractTemplatesListAsync();

        TemplateGroups = groups.Where(it => !string.IsNullOrEmpty(it.Name)).Select(it => new TemplateTreeItemProxy(it))
            .ToArray();

        UngroupedTemplates = groups.FirstOrDefault(it => string.IsNullOrEmpty(it.Name))?.Templates ?? [];
    }
}

public class TemplateSelectionFormViewModel<TTemplate>(ITemplatesService<TTemplate> templatesService)
    : TemplateSelectionFormViewModel(templatesService) where TTemplate : ITemplate;