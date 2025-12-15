namespace SparkTrack.Reactive;

using System.ComponentModel;

public interface IBehaviorObservable<out T> : IObservable<T>, INotifyPropertyChanged
{
    T Value { get; }
}