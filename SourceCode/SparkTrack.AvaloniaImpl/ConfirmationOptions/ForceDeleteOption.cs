namespace SparkTrack.AvaloniaImpl.ConfirmationOptions;

using ViewModels;

public class ForceDeleteOption() : SelectableViewModel<string>(
    "Удалить полностью в любом случае. (ВНИМАНИЕ! Все зависимые сущности будут также удалены!)"
);