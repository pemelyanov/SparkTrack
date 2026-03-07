using System.ComponentModel;
using System.Linq.Expressions;

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

    private readonly List<(string, Action)> m_propertyHandlers = [];

    #endregion

    #region LifeCycle

    protected ViewModelBase()
    {
        PropertyChanged += OnPropertyChanged;
        this.WhenActivated(disposables =>
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

                Disposable.Create(OnDeactivated).DisposeWith(disposables);
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
    protected virtual void OnFirstActivated(CompositeDisposable disposables)
    {
    }

    /// <summary>
    /// Хук, срабатывающий при каждой активации вьюмодели
    /// </summary>
    /// <param name="disposables">
    /// У этого объекта вызывается метод <see cref="IDisposable.Dispose" /> при деактивации вьюмодели
    /// </param>
    protected virtual void OnActivated(CompositeDisposable disposables)
    {
    }

    protected virtual void OnDeactivated()
    {
        
    }

    #endregion

    #region Properties

    /// <inheritdoc />
    public ViewModelActivator Activator { get; } = new();

    #endregion

    #region Methods

    public void DisposeWithViewModel(IDisposable disposable) => m_disposables.Add(disposable);

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing) return;

        m_disposables.Dispose();
        Activator.Dispose();
        PropertyChanged -= OnPropertyChanged;
        m_propertyHandlers.Clear();
    }

    protected IDisposable RegisterPropertyChangedHandler<TViewModel>(
        Expression<Func<TViewModel, object?>> propertySelector, Action handler)
    {
        string propertyName = GetPropertyName(propertySelector);

        (string, Action) namedHandler = (propertyName, handler);
        m_propertyHandlers.Add(namedHandler);

        return Disposable.Create(() => m_propertyHandlers.Remove(namedHandler));
    }

    private string GetPropertyName<TViewModel, TProperty>(
        Expression<Func<TViewModel, TProperty>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }

        // Обработка случаев с приведением типов (например, (int)Property)
        if (expression.Body is UnaryExpression unaryExpression &&
            unaryExpression.Operand is MemberExpression memberExpr)
        {
            return memberExpr.Member.Name;
        }

        throw new ArgumentException("Invalid property expression", nameof(expression));
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        foreach ((_, var handler) in m_propertyHandlers.Where(it => it.Item1 == e.PropertyName))
            handler();
    }

    #endregion
}