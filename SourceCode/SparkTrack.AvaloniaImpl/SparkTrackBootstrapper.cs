namespace SparkTrack.AvaloniaImpl;

using Windows.Main;
using API.AutofacModules;
using Autofac;
using Controls.Account;
using Controls.Attachment;
using Controls.AttachmentsPanel;
using Controls.ProjectEditForm;
using Controls.ProjectsFilter;
using Controls.UserEditForm;
using Core.Client.AutofacModules;
using Fanatiki.MVVM;
using Installers;
using Pages.Authorization;
using Pages.Feature;
using Pages.FeaturesList;
using Pages.ProjectsList;
using Pages.Users;

public class SparkTrackBootstrapper : BootstrapperBase<SparkTrackBootstrapper>
{
    protected override void RegisterViewModels(ContainerBuilder builder)
    {
        builder.RegisterType<MainWindowViewModel>().AsImplementedInterfaces().AsSelf().SingleInstance();
        builder.RegisterType<AuthorizationPageViewModel>().SingleInstance();
        builder.RegisterType<FeaturesListPageViewModel>().SingleInstance();
        builder.RegisterType<UsersPageViewModel>().SingleInstance();
        builder.RegisterType<AccountViewModel>().SingleInstance();
        builder.RegisterType<UserEditFormViewModel>();
        builder.RegisterType<FeaturePageViewModel>();
        builder.RegisterType<ProjectsListPageViewModel>().SingleInstance();
        builder.RegisterType<ProjectEditFormViewModel>();
        builder.RegisterType<ProjectsFilterViewModel>();
        builder.RegisterType<LocalAttachmentViewModel>();
        builder.RegisterType<RemoteAttachmentViewModel>();
        builder.RegisterType<AttachmentsPanelViewModel>();
    }

    protected override void RegisterServices(ContainerBuilder builder)
    {
        builder.RegisterType<MainWindow>().AsSelf().AsImplementedInterfaces().SingleInstance();

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