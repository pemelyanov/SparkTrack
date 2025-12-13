namespace Fanatiki.MVVM;

using System.Reflection;
using Autofac;
using ReactiveUI;
using Splat;
using Splat.Autofac;

/// <summary>
/// Объект для первоначальной настройки проекта. Регистрирует зависимости в DI контейнере
/// </summary>
public abstract class BootstrapperBase<TConcreteBootstrapper>
    where TConcreteBootstrapper : BootstrapperBase<TConcreteBootstrapper>, new()
{
    #region Fields

    private IContainer? m_container;

    #endregion

    #region LifeCycle

    /// <summary />
    protected BootstrapperBase() { }

    #endregion

    #region Properties

    /// <summary />
    public static TConcreteBootstrapper Instance { get; } = new();

    #endregion

    #region Methods

    /// <summary>
    /// Инициализирует DI контейнер
    /// </summary>
    /// <returns></returns>
    public IContainer InitializeIoC(bool withViewLocator = true, IEnumerable<Assembly>? viewAssemblies = null)
    {
        if (m_container is not null) return m_container;

        var builder = new ContainerBuilder();

        RegisterServices(builder);
        RegisterViewModels(builder);
        AutofacDependencyResolver resolver = RegisterAutofac(builder, withViewLocator, viewAssemblies);

        IContainer container = builder.Build();

        resolver.SetLifetimeScope(container);

        m_container = container;

        return container;
    }

    /// <summary>
    /// Метод для регистрации View
    /// </summary>
    /// <param name="builder"></param>
    protected virtual void RegisterViews(IMutableDependencyResolver builder) { }

    /// <summary>
    /// Метод для регистрации сервисов
    /// </summary>
    /// <param name="builder"></param>
    protected virtual void RegisterServices(ContainerBuilder builder) { }

    /// <summary>
    /// Метод для регистрации вьюмоделей
    /// </summary>
    /// <param name="builder"></param>
    protected virtual void RegisterViewModels(ContainerBuilder builder) { }

    private AutofacDependencyResolver RegisterAutofac(
        ContainerBuilder builder,
        bool withViewLocator,
        IEnumerable<Assembly>? viewAssemblies = null
    )
    {
        AutofacDependencyResolver resolver = builder.UseAutofacDependencyResolver();
        resolver.InitializeSplat();

        if (viewAssemblies is not null)
            foreach (Assembly viewAssembly in viewAssemblies)
                resolver.RegisterViewsForViewModels(viewAssembly);

        RegisterViews(resolver);
        builder.RegisterInstance(resolver);

        if (withViewLocator)
            RegisterViewLocator(builder);

        return resolver;
    }

    private static void RegisterViewLocator(ContainerBuilder builder)
    {
        builder.RegisterInstance(ViewLocator.Current).As<IViewLocator>().SingleInstance();
    }

    #endregion
}