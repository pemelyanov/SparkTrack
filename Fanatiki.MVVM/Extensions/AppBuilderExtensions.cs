namespace Fanatiki.MVVM.Extensions;

using System.Reflection;
using Avalonia;

/// <summary>
/// Расширения для <see cref="AppBuilder" />
/// </summary>
public static class AppBuilderExtensions
{
    #region Methods

    /// <summary>
    /// Инициализирует DI контейнер после инициализации приложения
    /// </summary>
    /// <param name="app">Билдер приложения</param>
    /// <param name="viewAssemblies">Assembly, содержащие View и ViewModels для регистрации во ViewLocator</param>
    /// <returns></returns>
    public static AppBuilder UseBootstrapper<TBootstrapper>(
        this AppBuilder app,
        IEnumerable<Assembly>? viewAssemblies = null
    )
        where TBootstrapper : BootstrapperBase<TBootstrapper>, new() => app.AfterPlatformServicesSetup(
        _ => BootstrapperBase<TBootstrapper>.Instance.InitializeIoC(viewAssemblies: viewAssemblies)
    );

    #endregion
}