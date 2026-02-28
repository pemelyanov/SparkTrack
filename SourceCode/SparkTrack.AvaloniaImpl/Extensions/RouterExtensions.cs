namespace SparkTrack.AvaloniaImpl.Extensions;

using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading;
using DynamicData;
using ReactiveUI;

/// <summary>
/// Расширения для <see cref="RoutingState" />
/// </summary>
public static class RouterExtensions
{
    /// <summary>
    /// Производит переход к странице в UI потоке
    /// </summary>
    /// <param name="router"></param>
    /// <param name="viewModel"></param>
    public static void NavigateOnUIThread(this RoutingState router, IRoutableViewModel viewModel) =>
        Dispatcher.UIThread.Invoke(() => router.Navigate.Execute(viewModel));

    /// <summary>
    /// Производит переход к предыдущей странице в UI потоке
    /// </summary>
    /// <param name="router"></param>
    public static void BackOnUIThread(this RoutingState router) =>
        Dispatcher.UIThread.Invoke(() => router.NavigateBack.Execute());

    /// <summary>
    /// Удаляет страницы сверху стека навигации до тех пор, пока не дойдет до указанной страницы, либо добавляет ее в стек,
    /// если ее еще там нет
    /// </summary>
    /// <param name="router"></param>
    /// <param name="viewModel"></param>
    public static void PopToOnUIThread(this RoutingState router, IRoutableViewModel viewModel)
    {
        IRoutableViewModel[] stack = router.NavigationStack.ToArray();

        if (stack.Length > 0 && stack[^1] == viewModel) return;

        int existingViewModelIndex = stack.IndexOf(viewModel);

        if (existingViewModelIndex < 0)
        {
            router.NavigateOnUIThread(viewModel);

            return;
        }

        IEnumerable<IRoutableViewModel> itemsToRemove = stack.Skip(existingViewModelIndex + 1);

        Dispatcher.UIThread.Invoke(() =>
        {
            var suspension = (router.NavigationStack as SuspendableObservableCollection<IRoutableViewModel>)?
                .SuspendNotifications();
            router.NavigationStack.RemoveMany(itemsToRemove);
            suspension?.Dispose();
        });
    }

    /// <summary>
    /// Поднимает указанную страницу наверх стека навигации, либо добавляет ее в стек, если ее там нет
    /// </summary>
    /// <param name="router"></param>
    /// <param name="viewModel"></param>
    public static void BringUpOnUIThread(this RoutingState router, IRoutableViewModel viewModel)
    {
        IRoutableViewModel[] stack = router.NavigationStack.ToArray();

        if (stack.Length > 0 && stack[^1] == viewModel) return;

        IRoutableViewModel? existingViewModel = stack.FirstOrDefault(it => it == viewModel);

        var suspension = (router.NavigationStack as SuspendableObservableCollection<IRoutableViewModel>)?
            .SuspendNotifications(false);
        
        if (existingViewModel is not null)
            Dispatcher.UIThread.Invoke(() => router.NavigationStack.Remove(existingViewModel));
        
        suspension?.Dispose();

        router.NavigateOnUIThread(viewModel);
    }

    /// <summary>
    /// Очищает стек и добавляет в него указанную страницу
    /// </summary>
    /// <param name="router"></param>
    /// <param name="viewModel"></param>
    public static void ResetToOnUIThread(this RoutingState router, IRoutableViewModel viewModel) =>
        Dispatcher.UIThread.Invoke(() =>
        {
            var suspension = (router.NavigationStack as SuspendableObservableCollection<IRoutableViewModel>)?
                .SuspendNotifications();
            
            router.NavigateAndReset.Execute(viewModel);
            
            suspension?.Dispose();
        });
    
    /// <summary>
    /// Производит навигацию на шаг назад, если в стеке больше одного элемента
    /// </summary>
    /// <param name="router"></param>
    public static void SafeBackOnUIThread(this RoutingState router) =>
        Dispatcher.UIThread.Invoke(() =>
        {
            if (router.NavigationStack.Count < 2) return;

            router.NavigateBack.Execute();
        });
}