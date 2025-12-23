namespace SparkTrack.AvaloniaImpl.Controls.SubTask;

using Core.Shared.Data.Entities;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI.Fody.Helpers;

public class SubTaskViewModel(IReadOnlyList<User> availableEmployees, Action<SubTaskViewModel> onDelete) : ViewModelBase
{
    [Reactive]
    public bool IsInEditMode { get; set; }

    [Reactive]
    public string Name { get; set; } = string.Empty;

    public IReadOnlyList<User> AvailableEmployees => availableEmployees;

    [Reactive]
    public User? SelectedEmployee { get; set; }

    [Reactive]
    public DateTime Deadline { get; set; } = DateTime.Now;

    [Reactive]
    public float Price { get; set; }

    public void Delete() => onDelete(this);
}