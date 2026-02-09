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
using Controls.UsersFilter;
using Core.Client.AutofacModules;
using Core.Client.Events;
using Core.Shared.Eventing;
using Fanatiki.MVVM;
using Fanatiki.Updating.GitHub.Services;
using Fanatiki.Updating.Services;
using Installers;
using Microsoft.Extensions.Configuration;
using Pages.AdminFinance;
using Pages.AdminFinance.Tabs.PaymentsHistory;
using Pages.AdminFinance.Tabs.PendingPayments;
using Pages.Authorization;
using Pages.Feature;
using Pages.FeaturesList;
using Pages.ProjectsList;
using Pages.Update;
using Pages.Users;
using Services.AttachmentsPathCache;

public class SparkTrackBootstrapper : BootstrapperBase<SparkTrackBootstrapper>
{
    private static readonly IConfiguration s_configuration = InitializeConfiguration();

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
        builder.RegisterType<UserFilterViewModel>();
        builder.RegisterType<ClipboardAttachmentViewModel>();
    }

    protected override void RegisterServices(ContainerBuilder builder)
    {
        builder.RegisterType<MainWindow>().AsSelf().AsImplementedInterfaces().SingleInstance();

        builder.RegisterAvaloniaServices();
        builder.RegisterModule<CoreClientModule>();
        // TODO: Вынести путь до настроек в файл конфигурации
        builder.RegisterModule(
            new APIModule(
                s_configuration.GetRequiredSection("ApiBaseUrl").Get<string>()
                ?? throw new InvalidOperationException("Api base url not found"),
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "SparkTrack",
                    "tokens.json"
                )
            )
        );
        builder.RegisterType<JsonAttachmentsPathCache>().AsImplementedInterfaces().SingleInstance();
        
        RegisterUpdatingIfNeeded(builder);
    }

    private void RegisterUpdatingIfNeeded(ContainerBuilder builder)
    {
        var updatingSection = s_configuration.GetSection("Updating");

        if (!updatingSection.Exists()) return;

        builder.RegisterType<UpdatePageViewModel>().SingleInstance();
        builder.RegisterType<UpdateUnpackerService>()
            .WithParameters(
                [
                    new NamedParameter(
                        "updatedUnpackerPath",
                        Path.Combine(Environment.CurrentDirectory, "SparkTrack.Unpacker.exe")
                    ),
                    new NamedParameter(
                        "currentUnpackerPath",
                        Path.Combine(Environment.CurrentDirectory, "SparkTrack.Unpacker.Current.exe")
                    )
                ]
            )
            .As<IUpdateUnpackerService>()
            .SingleInstance();

        builder.RegisterType<UpdateService>()
            .WithParameters(
                [
                    new NamedParameter("applicationRootPath", Environment.CurrentDirectory)
                ]
            )
            .As<IUpdateUnpackerService>()
            .SingleInstance();

        builder.RegisterType<GitHubUpdateService>()
            .WithParameters(
                [
                    new NamedParameter("repoName", updatingSection.GetRequiredSection("RepoName").Get<string>()),
                    new NamedParameter("repoOwner", updatingSection.GetRequiredSection("RepoOwner").Get<string>()),
                    new NamedParameter("accessToken", updatingSection.GetRequiredSection("AccessToken").Get<string>()),
                ]
            )
            .As<IUpdateUnpackerService>()
            .SingleInstance();
    }

    private static IConfiguration InitializeConfiguration()
    {
        return new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .Build();
    }
}