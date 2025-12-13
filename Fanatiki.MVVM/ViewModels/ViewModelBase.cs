namespace Fanatiki.MVVM.ViewModels;

using System.Reactive.Disposables;
using ReactiveUI;
using ReactiveUI.Validation.Helpers;

public class ViewModelBase : ReactiveValidationObject, IActivatableViewModel
{
    #region Fields

    /// <summary>
    /// Объект для объединения подписок. Освобождается при освобождении вьюмодели
    /// </summary>
    protected readonly CompositeDisposable m_disposables = new();

    protected bool m_hasFirstActivation;

    #endregion

    #region LifeCycle
    
    protected ViewModelBase()
    {
        this.WhenActivated(
            disposables =>
            {
                if (!m_hasFirstActivation)
                {
                    OnFirstActivated(m_disposables);
                    OnActivated(disposables);
                    m_hasFirstActivation = true;
                }
                else
                {
                    OnActivated(disposables);
                }
            }
        );
    }

    /// <summary>
    /// Хук, срабатывающий при первой активации вьюмодели
    /// </summary>
    /// <param name="disposables">
    /// У этого объекта вызывается метод <see cref="IDisposable.Dispose" /> при вызове метода <see cref="Dispose" /> у
    /// вьюмоодели
    /// </param>
    protected virtual void OnFirstActivated(CompositeDisposable disposables) { }

    /// <summary>
    /// Хук, срабатывающий при каждой активации вьюмодели
    /// </summary>
    /// <param name="disposables">
    /// У этого объекта вызывается метод <see cref="IDisposable.Dispose" /> при деактивации вьюмодели
    /// </param>
    protected virtual void OnActivated(CompositeDisposable disposables) { }

    #endregion

    #region Properties

    /// <inheritdoc />
    public ViewModelActivator Activator { get; } = new();

    #endregion

    #region Methods

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing) return;

        m_disposables.Dispose();
        Activator.Dispose();
    }

    #endregion
}