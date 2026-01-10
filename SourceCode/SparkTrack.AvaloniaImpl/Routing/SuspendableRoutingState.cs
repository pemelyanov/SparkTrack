namespace SparkTrack.AvaloniaImpl.Routing;

using ReactiveUI;
using System.Reactive.Concurrency;
using System.Reflection;

public class SuspendableRoutingState : RoutingState
{
    public SuspendableRoutingState(IScheduler? scheduler = null) : base(scheduler)
    {
        var navigationStack = new SuspendableObservableCollection<IRoutableViewModel>();

        NavigationStack = navigationStack;
        
        // HACK: Вызываем приватный метод для корректной переинициализации свойств, завязанных на NavigationStack
        var methodInfo = typeof(RoutingState).GetMethods(
            BindingFlags.NonPublic | BindingFlags.Instance)
            .First(it => it.Name == "SetupRx" && it.GetParameters().Length == 0);

        methodInfo.Invoke(this, null);
    }
}