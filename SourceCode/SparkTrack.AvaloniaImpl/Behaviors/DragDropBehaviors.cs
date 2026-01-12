namespace SparkTrack.AvaloniaImpl.Behaviors;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;

public class DragDropBehaviors
{
    private const string DragOverClass = ":drag-over";
    
    static DragDropBehaviors()
    {
        AddDragOverClassProperty.Changed.AddClassHandler<Control>(OnAddDragOverClassChanged);
    }

    private static void OnAddDragOverClassChanged(Control obj, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.NewValue is false)
        {
            obj.RemoveHandler(DragDrop.DragEnterEvent, FilesPanel_OnDragEnter);
            obj.RemoveHandler(DragDrop.DragLeaveEvent, FilesPanel_OnDragLeave);
            obj.RemoveHandler(DragDrop.DropEvent, FilesPanel_OnDragLeave);
            
            return;
        }

        if (args.NewValue is true && args.OldValue != args.NewValue)
        {
            obj.AddHandler(DragDrop.DragEnterEvent, FilesPanel_OnDragEnter);
            obj.AddHandler(DragDrop.DragLeaveEvent, FilesPanel_OnDragLeave);
            obj.AddHandler(DragDrop.DropEvent, FilesPanel_OnDragLeave);
        }
    }
    
    public static readonly AttachedProperty<bool> AddDragOverClassProperty = AvaloniaProperty.RegisterAttached<FocusBehaviors, Control, bool>(
        "AddDragOverClass", false, false, BindingMode.OneTime);

    public static bool GetAddDragOverClass(Control obj) => obj.GetValue(AddDragOverClassProperty);

    public static void SetAddDragOverClass(Control obj, bool value) => obj.SetValue(AddDragOverClassProperty, value);


    private static void FilesPanel_OnDragEnter(object? sender, DragEventArgs e)
    {
        if(sender is not Control panel) return;
        
        ((IPseudoClasses)panel.Classes).Add(DragOverClass);
    }
    
    private static void FilesPanel_OnDragLeave(object? sender, DragEventArgs e)
    {
        if(sender is not Control panel) return;
        
        ((IPseudoClasses)panel.Classes).Remove(DragOverClass);
    }
}