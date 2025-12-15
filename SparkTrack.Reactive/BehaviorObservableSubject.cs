namespace SparkTrack.Reactive;

using System.ComponentModel;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;

public class BehaviorObservableSubject<T>(T initialValue) : IBehaviorObservable<T>, ISubject<T>, IDisposable
{
    private readonly BehaviorSubject<T> m_inner = new(initialValue);

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Dispose()
    {
        m_inner.Dispose();
    }

    public T Value
    {
        get => m_inner.Value;
        set => OnNext(value);
    }

    public IDisposable Subscribe(IObserver<T> observer) => m_inner.Subscribe(observer);

    public void OnCompleted() => m_inner.OnCompleted();

    public void OnError(Exception error) => m_inner.OnError(error);

    public void OnNext(T value)
    {
        m_inner.OnNext(value);
        OnPropertyChanged(nameof(Value));
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}