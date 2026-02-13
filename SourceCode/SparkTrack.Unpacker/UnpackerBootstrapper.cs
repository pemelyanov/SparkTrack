namespace SparkTrack.Unpacker;

using Windows.Main;
using Autofac;
using Fanatiki.MVVM;

internal class UnpackerBootstrapper : BootstrapperBase<UnpackerBootstrapper>
{
    protected override void RegisterServices(ContainerBuilder builder)
    {
        builder.RegisterInstance(Program.Options).SingleInstance();
    }

    protected override void RegisterViewModels(ContainerBuilder builder)
    {
        builder.RegisterType<MainWindowViewModel>().SingleInstance();
    }
}