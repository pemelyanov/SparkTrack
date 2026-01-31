namespace SparkTrack.AvaloniaImpl;

using Windows.Main;
using API.AutofacModules;
using Autofac;
using Controls.Account;
using Controls.Attachment;
using Controls.AttachmentsPanel;
using Controls.BonusForm;
using Controls.ChangePasswordForm;
using Controls.Comment;
using Controls.CommentEdit;
using Controls.ProjectEditForm;
using Controls.ProjectsFilter;
using Controls.SubTask;
using Controls.UserEditForm;
using Core.Client.AutofacModules;
using Core.Client.Events;
using Core.Shared.Eventing;
using Fanatiki.MVVM;
using Installers;
using Pages.AdminFinance;
using Pages.AdminFinance.Tabs.PaymentsHistory;
using Pages.AdminFinance.Tabs.PendingPayments;
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
        builder.RegisterType<UsersPageViewModel>().As<IEventHandler<LogoutEvent>>().AsSelf().SingleInstance();
        builder.RegisterType<AccountViewModel>().SingleInstance();
        builder.RegisterType<UserEditFormViewModel>();
        builder.RegisterType<FeaturePageViewModel>();
        builder.RegisterType<ProjectsListPageViewModel>().SingleInstance();
        builder.RegisterType<ProjectEditFormViewModel>();
        builder.RegisterType<ProjectsFilterViewModel>();
        builder.RegisterType<LocalAttachmentViewModel>();
        builder.RegisterType<RemoteAttachmentViewModel>();
        builder.RegisterType<AttachmentsPanelViewModel>();
        builder.RegisterType<CommentEditViewModel>();
        builder.RegisterType<CommentViewModel>();
        builder.RegisterType<SubTaskViewModel>();
        builder.RegisterType<ChangePasswordFormViewModel>();
        builder.RegisterType<AdminFinancePageViewModel>();
        builder.RegisterType<BonusFormViewModel>();
        builder.RegisterType<PendingPaymentsViewModel>();
        builder.RegisterType<PaymentsHistoryViewModel>();
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