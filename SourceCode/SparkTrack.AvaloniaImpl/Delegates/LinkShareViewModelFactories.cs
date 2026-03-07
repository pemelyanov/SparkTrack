using SparkTrack.AvaloniaImpl.Windows.LinkShare;

namespace SparkTrack.AvaloniaImpl.Delegates;

public delegate LinkShareViewModel LinkShareViewModelFactory(Func<Task<string>> linkFactory);