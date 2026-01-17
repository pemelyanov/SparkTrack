namespace SparkTrack.AvaloniaImpl.Delegates;

using Controls.SubTask;
using Core.Shared.Data.Entities;
using SubTask = Core.Shared.Data.Entities.SubTask;

public delegate SubTaskViewModel SubTaskViewModelFactory(SubTask? subTask,
                                                         IObservable<IReadOnlyList<User>> availableEmployees,
                                                         Action<SubTaskViewModel> onDelete);