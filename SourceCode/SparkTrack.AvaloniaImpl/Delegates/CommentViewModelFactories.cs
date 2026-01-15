namespace SparkTrack.AvaloniaImpl.Delegates;

using Controls.Comment;
using Comment = Core.Shared.Data.Entities.Comment;

public delegate CommentViewModel CommentViewModelFactory(Comment model, Func<CommentViewModel, Task> onDelete);