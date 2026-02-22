namespace SparkTrack.AvaloniaImpl.Windows.LinkShare;

using System.Reactive;
using System.Reactive.Disposables;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Services.Clipboard;
using ViewModels;

public class LinkShareViewModel : DialogViewModelBase
{
    private readonly IClipboardService m_clipboardService;

    public LinkShareViewModel(Func<Task<string>> linkFactory, IClipboardService clipboardService)
    {
        m_clipboardService = clipboardService;
        GetLinkCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            Link = await linkFactory();
        });
    }

    protected override void OnFirstActivated(CompositeDisposable disposables)
    {
        base.OnFirstActivated(disposables);

        GetLinkCommand.Execute().Subscribe().DisposeWith(disposables);
    }

    [Reactive]
    public string Link { get; private set; } = string.Empty;

    public ReactiveCommand<Unit, Unit> GetLinkCommand { get; }

    public Task CopyAsync() => m_clipboardService.SaveToClipboardAsync(Link, "Ссылка скопирована");
}