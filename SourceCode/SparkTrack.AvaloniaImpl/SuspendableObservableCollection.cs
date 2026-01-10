namespace SparkTrack.AvaloniaImpl;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Disposable = System.Reactive.Disposables.Disposable;

/// <summary>
/// ObservableCollection с возможностью приостановки уведомлений
/// </summary>
/// <typeparam name="T">Хранимый тип</typeparam>
public class SuspendableObservableCollection<T> : ObservableCollection<T>
{
    #region Fields

    private          bool   m_notificationsSuspended;
    private          bool   m_suspendNotifications;
    private readonly object m_lockObject = new();

    #endregion

    #region LifeCycle

    /// <inheritdoc />
    public SuspendableObservableCollection() { }

    /// <inheritdoc />
    public SuspendableObservableCollection(IEnumerable<T> source) : base(source) { }

    #endregion

    #region Properties

    /// <summary>
    /// Указывает что коллекция в процессе обновления
    /// </summary>
    public bool IsChanging { get; private set; }

    #endregion

    #region Methods

    /// <summary>
    /// Инициирует событие обновления коллекции
    /// </summary>
    public void NotifyCollectionChanged() =>
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

    /// <summary>
    /// Приостанавливает уведомления об изменении коллекции до тех пор, пока IDisposable не будет Disposed
    /// </summary>
    /// <param name="notifyAfterDispose">Уведомить после отмены приостановки уведомлений</param>
    /// <returns>Объект для прекращения приостановки</returns>
    public IDisposable SuspendNotifications(bool notifyAfterDispose = true)
    {
        lock (m_lockObject)
        {
            if (m_suspendNotifications) throw new InvalidOperationException("Notifications already suspended");
            
            m_suspendNotifications = true;

            return Disposable.Create(
                () =>
                {
                    m_suspendNotifications = false;

                    if (!m_notificationsSuspended || !notifyAfterDispose) return;

                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    m_notificationsSuspended = false;
                }
            );
        }
    }

    /// <inheritdoc />
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        if (m_suspendNotifications)
        {
            m_notificationsSuspended = true;
            return;
        }

        IsChanging = true;
        base.OnCollectionChanged(e);
        IsChanging = false;
    }

    #endregion
}