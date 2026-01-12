namespace SparkTrack.AvaloniaImpl.Installers;

using Autofac;
using Services.LocalFilesManager;
using Services.NavigationListResolver;

public static class ServiceInstaller
{
    public static void RegisterAvaloniaServices(this ContainerBuilder builder)
    {
        builder.RegisterType<NavigationListResolver>().As<INavigationListResolver>().SingleInstance();

        builder.RegisterType<LocalFilesManager>().AsImplementedInterfaces().SingleInstance();
    }
}