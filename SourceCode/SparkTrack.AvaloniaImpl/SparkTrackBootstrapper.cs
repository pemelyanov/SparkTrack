namespace SparkTrack.AvaloniaImpl;

using Windows.Main;
using API.AutofacModules;
using Autofac;
using Controls.Account;
using Core.Client.AutofacModules;
using Fanatiki.MVVM;
using Installers;
using Pages.Authorization;
using Pages.Features;
using ReactiveUI;

public class SparkTrackBootstrapper : BootstrapperBase<SparkTrackBootstrapper>
{
    protected override void RegisterViewModels(ContainerBuilder builder)
    {
        builder.RegisterType<MainWindowViewModel>().As<IScreen>().AsSelf().SingleInstance();
        builder.RegisterType<AuthorizationPageViewModel>().SingleInstance();
        builder.RegisterType<FeaturesPageViewModel>().SingleInstance();
        builder.RegisterType<AccountViewModel>().SingleInstance();
    }

    protected override void RegisterServices(ContainerBuilder builder)
    {
        builder.RegisterType<MainWindow>().AsSelf().SingleInstance();

        builder.RegisterAvaloniaServices();
        builder.RegisterModule<CoreClientModule>();
        // TODO: Вынести BaseAPI в файл конфигурации
        // TODO: Вынести путь до настроек в файл конфигурации
        builder.RegisterModule(
            new APIModule(
                "http://localhost:5196/",
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "SparkTrack",
                    "tokens.json"
                )
            )
        );
    }
}