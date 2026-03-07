using SparkTrack.AvaloniaImpl.Controls.TemplateSelectionForm;
using SparkTrack.AvaloniaImpl.Data.Configurations;
using SparkTrack.AvaloniaImpl.Data.Templates;
using SparkTrack.AvaloniaImpl.Pages.Settings;
using SparkTrack.AvaloniaImpl.Services.Explorer;
using SparkTrack.AvaloniaImpl.Services.Templates;
using SparkTrack.Core.Client.Extensions;

namespace SparkTrack.AvaloniaImpl;

using Windows.LinkShare;
using Windows.Main;
using Windows.UserSelection;
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
using Controls.TemplateSaveForm;
using Controls.UserAddForm;
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
using Minerals.StringCases;
using Pages.AdminFinance;
using Pages.AdminFinance.Tabs.PaymentsHistory;
using Pages.AdminFinance.Tabs.PendingPayments;
using Pages.Authorization;
using Pages.Feature;
using Pages.FeaturesList;
using Pages.ProjectsList;
using Pages.Update;
using Pages.UsersList;
using ReactiveUI;
using Services.AttachmentsPathCache;
using Services.DeepLinkNavigation;
using Splat;

public class SparkTrackBootstrapper : BootstrapperBase<SparkTrackBootstrapper>
{
    private static readonly IConfiguration s_configuration = InitializeConfiguration();

    protected override void RegisterViews(IMutableDependencyResolver builder)
    {
        RegisterTemplateViewModel<SubTaskTemplate>(builder);
        RegisterTemplateViewModel<FeatureTemplate>(builder);
    }

    protected override void RegisterViewModels(ContainerBuilder builder)
    {
        builder.RegisterType<MainWindowViewModel>().AsImplementedInterfaces().AsSelf().SingleInstance();
        builder.RegisterType<AuthorizationPageViewModel>().SingleInstance();
        builder.RegisterType<FeaturesListPageViewModel>().SingleInstance();
        builder.RegisterType<UsersListPageViewModel>().As<IEventHandler<LogoutEvent>>().AsSelf().SingleInstance();
        builder.RegisterType<AccountViewModel>().SingleInstance();
        builder.RegisterType<UserAddFormViewModel>();
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
        builder.RegisterType<UserEditFormViewModel>();
        builder.RegisterType<SettingsPageViewModel>().SingleInstance();
        builder.RegisterGeneric(typeof(TemplateSaveFormViewModel<>));
        builder.RegisterGeneric(typeof(TemplateSelectionFormViewModel<>));
        builder.RegisterType<LinkShareViewModel>();
        builder.RegisterType<UserSelectionViewModel>();
    }

    protected override void RegisterServices(ContainerBuilder builder)
    {
        builder.RegisterInstance(s_configuration).As<IConfiguration>().SingleInstance();
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
        builder.RegisterType<WindowsExplorerService>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterJsonConfiguration<InterfaceConfiguration>(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SparkTrack",
            "Settings",
            "interface-configuration.json"
        ));

        var configsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SparkTrack",
            "Configs"
        );
        
        builder.RegisterJsonConfiguration<WindowStateConfig>(Path.Combine(
            configsFolder,
            "window-state.json"
        ));
        
        builder.RegisterJsonConfiguration<FeaturesPageConfig>(Path.Combine(
            configsFolder,
            "features-page.json"
        ));
        
        builder.RegisterJsonConfiguration<AdminPendingPaymentsPageConfig>(Path.Combine(
            configsFolder,
            "admin-pending-payments-page.json"
        ));
        
        builder.RegisterJsonConfiguration<AdminPaymentsHistoryPageConfig>(Path.Combine(
            configsFolder,
            "admin-payments-history-page.json"
        ));

        RegisterUpdatingIfNeeded(builder);
        
        RegisterTemplateService<SubTaskTemplate>(builder , "SubTasks");
        RegisterTemplateService<FeatureTemplate>(builder, "Features");

        builder.RegisterType<DeepLinkNavigationService>().AsImplementedInterfaces().SingleInstance();
    }

    private void RegisterUpdatingIfNeeded(ContainerBuilder builder)
    {
        var updatingSection = s_configuration.GetSection("Updating");

        if (!updatingSection.Exists()) return;

        builder.RegisterType<UpdatePageViewModel>().SingleInstance();
        builder.RegisterType<InnoSetupInstallerUnpackerService>()
            .AsImplementedInterfaces()
            .SingleInstance();

        builder.RegisterType<UpdateService>()
            .WithParameters(
                [
                    new NamedParameter("applicationRootPath", Environment.CurrentDirectory)
                ]
            )
            .AsImplementedInterfaces()
            .SingleInstance();

        builder.RegisterType<GitHubUpdateService>()
            .WithParameters(
                typeof(GitHubUpdateService).GetConstructors()[0]
                    .GetParameters()
                    .Select(parameter => new NamedParameter(
                            parameter.Name!,
                            updatingSection.GetRequiredSection(parameter.Name!.ToPascalCase()).Get<string>()
                        )
                    )
            )
            .AsImplementedInterfaces()
            .SingleInstance();
    }

    private static IConfiguration InitializeConfiguration()
    {
        return new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .Build();
    }

    private void RegisterTemplateService<TTemplate>(ContainerBuilder builder, string categoryName)
        where TTemplate : ITemplate
    {
        builder.RegisterType<JsonTemplatesService<TTemplate>>()
            .WithParameter(new TypedParameter(typeof(string), categoryName))
            .As<ITemplatesService<TTemplate>>()
            .As<IAbstractTemplatesService>()
            .SingleInstance();
    }
    
    private void RegisterTemplateViewModel<TTemplate>(IMutableDependencyResolver builder)
        where TTemplate : ITemplate
    {
        builder.Register(() => new TemplateSaveForm(), typeof(IViewFor<TemplateSaveFormViewModel<TTemplate>>));
        builder.Register(() => new TemplateSelectionForm(), typeof(IViewFor<TemplateSelectionFormViewModel<TTemplate>>));
    }
}