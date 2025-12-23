namespace SparkTrack.AvaloniaImpl.Controls.Comment;

using Fanatiki.MVVM.ViewModels;
using CommentModel = Core.Shared.Data.Entities.Comment;

public class CommentViewModel(CommentModel model) : ViewModelBase
{
    public CommentModel Model { get; } = model;
}