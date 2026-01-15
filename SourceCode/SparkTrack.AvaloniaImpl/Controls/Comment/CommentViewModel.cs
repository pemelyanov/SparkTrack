namespace SparkTrack.AvaloniaImpl.Controls.Comment;

using CommentEdit;
using Fanatiki.MVVM.ViewModels;
using ReactiveUI.Fody.Helpers;
using CommentModel = Core.Shared.Data.Entities.Comment;

public class CommentViewModel(CommentModel model, Func<CommentModel?, CommentEditViewModel> editViewModelFactory) : ViewModelBase
{
    [Reactive]
    public CommentModel Model { get; private set; } = model;
    
    public CommentEditViewModel? EditViewModel { get; private set; }

    public void Edit()
    {
        EditViewModel = editViewModelFactory.Invoke(Model);
    }

    public async Task SaveAsync()
    {
        EditViewModel = null;
    }
}