using System.Reactive.Disposables;
using System.Reactive.Threading.Tasks;
using SparkTrack.AvaloniaImpl.Services.DialogHost;
using SparkTrack.AvaloniaImpl.Windows.TextInput;

namespace SparkTrack.AvaloniaImpl.Controls.TemplateSaveForm;

using System.Reactive;
using Data.Templates;
using Extensions;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Services.Templates;
using TemplatesList;
using ViewModels;

public class TemplateSaveFormViewModel : DialogViewModelBase
{
    private readonly IAbstractTemplatesService m_templatesService;
    private readonly ITemplate                 m_template;
    private readonly IDialogService            m_dialogService;

    public TemplateSaveFormViewModel(
        IAbstractTemplatesService templatesService,
        ITemplate template,
        IDialogService dialogService
    )
    {
        m_templatesService = templatesService;
        m_template = template;
        m_dialogService = dialogService;

        ReloadCommand = ReactiveCommand.CreateFromTask(ReloadAsync);

        RegisterPropertyChangedHandler<TemplateSaveFormViewModel>(it => it.SelectedTemplate, OnSelectedGroupChanged);
    }

    protected override void OnActivated(CompositeDisposable disposables)
    {
        base.OnActivated(disposables);

        ReloadCommand.Execute().Subscribe().DisposeWith(disposables);
    }

    [Reactive]
    public string TemplateName { get; set; } = string.Empty;

    [Reactive]
    public IReadOnlyList<TemplateTreeItemProxy> TemplateGroups { get; private set; } = [];

    [Reactive]
    public IReadOnlyList<ITemplate> UngroupedTemplates { get; private set; } = [];

    [Reactive]
    public ITemplateGroup? SelectedGroup { get; set; }

    [Reactive]
    public ITemplate? SelectedTemplate { get; set; }

    public ReactiveCommand<Unit, Unit> ReloadCommand { get; }

    public async Task CreateGroupAsync()
    {
        var groupNameInputViewModel = new TextInputDialogViewModel(
            "Введите название группы:",
            "Создание группы",
            acceptText: "Ок",
            cancelText: "Отмена"
        );

        if (await m_dialogService.ShowAsync(groupNameInputViewModel) is not true ||
            string.IsNullOrEmpty(groupNameInputViewModel.Text)) return;

        await m_templatesService.AddGroupAsync(groupNameInputViewModel.Text);

        await ReloadCommand.Execute().ToTask();
    }

    public async Task SaveTemplateAsync()
    {
        if (string.IsNullOrWhiteSpace(TemplateName)) return;

        var isOverride = SelectedGroup is null
            ? UngroupedTemplates.Any(it => it.TemplateName == TemplateName)
            : TemplateGroups.Any(it =>
                it.Group == SelectedGroup && it.Children.Any(c => c.Template?.TemplateName == TemplateName)
            );

        if (isOverride && !await m_dialogService.ConfirmAsync(
            "Шаблон с таким названием уже существует, он будет перезаписан. Продолжить?",
            "Перезапись шаблона"
        )) return;

        m_template.TemplateName = TemplateName;
        await m_templatesService.AddAsync(m_template, SelectedGroup?.Name ?? string.Empty);
        Close(true);
    }

    private async Task ReloadAsync()
    {
        var groups = await m_templatesService.GetAbstractTemplatesListAsync();

        TemplateGroups = groups.Where(it => !string.IsNullOrEmpty(it.Name))
            .Select(it => new TemplateTreeItemProxy(it))
            .ToArray();

        UngroupedTemplates = groups.FirstOrDefault(it => string.IsNullOrEmpty(it.Name))?.Templates ?? [];
    }
    
    
    private void OnSelectedGroupChanged()
    {
        if(SelectedTemplate is null) return;

        TemplateName = SelectedTemplate.TemplateName;
    }
}

public class TemplateSaveFormViewModel<TTemplate>(
    ITemplatesService<TTemplate> templatesService,
    TTemplate template,
    IDialogService dialogService
)
    : TemplateSaveFormViewModel(templatesService, template, dialogService) where TTemplate : ITemplate;