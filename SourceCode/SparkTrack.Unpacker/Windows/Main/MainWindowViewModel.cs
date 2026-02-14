namespace SparkTrack.Unpacker.Windows.Main;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Fanatiki.MVVM.ViewModels;
using Fanatiki.ZIP;
using NLog;
using ReactiveUI.Fody.Helpers;
using ILogger = NLog.ILogger;

internal class MainWindowViewModel(UnpackOptions options) : ViewModelBase
{
    #region Fields

    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();

    #endregion

    #region LifeCycle

    /// <inheritdoc />
    protected override void OnFirstActivated(CompositeDisposable disposables)
    {
        s_logger.Info("Unpacker window activated");
        base.OnFirstActivated(disposables);

        UnpackAsync().ToObservable().Subscribe().DisposeWith(disposables);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Текущее состояние распаковки
    /// </summary>
    [Reactive]
    public string CurrentState { get; private set; } = string.Empty;

    #endregion

    #region Methods

    private Task UnpackAsync() => Task.Run(
        async () =>
        {
            try
            {
                s_logger.Info("Unpacking with options {options}", options);
                string launcherPath = options.LauncherPath;
                string updateZip = options.UpdateZipPath;

                CurrentState = "Ждем завершения работы программы...";
                // Дождаться закрытия лаунчера
                while (Process.GetProcessesByName(options.LauncherProcessName).Any())
                {
                    s_logger.Info("Process alive, waiting for exit...");
                    await Task.Delay(500);
                }

                CurrentState = "Распаковываем обновление...";
                s_logger.Info("Unpacking update...");
                ZipUtils.ExtractZipWithOverwrite(updateZip, launcherPath);

                File.Delete(updateZip);

                CurrentState = "Запускаем программу...";
                s_logger.Info("Starting new program...");

                var startInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(launcherPath, $"{options.LauncherProcessName}.exe"),
                    WorkingDirectory = launcherPath,
                };

                Process.Start(startInfo);

                Environment.Exit(0);
            }
            catch (Exception e)
            {
                s_logger.Error(e);
                CurrentState = e.Message;
                await Task.Delay(2000);
                Environment.Exit(1);
            }
        }
    );

    #endregion
}