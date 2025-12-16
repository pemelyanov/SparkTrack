namespace SparkTrack.AvaloniaImpl;

using Windows.Main;
using API.AutofacModules;
using Autofac;
using Core.Client.Installers;
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
    }

    protected override void RegisterServices(ContainerBuilder builder)
    {
        builder.RegisterType<MainWindow>().AsSelf().SingleInstance();

        builder.RegisterAvaloniaServices();
        builder.RegisterClientCoreServices();
        // TODO: Вынести BaseAPI в файл конфигурации
        builder.RegisterModule(new APIModule("http://localhost:5196/"));
    }
}