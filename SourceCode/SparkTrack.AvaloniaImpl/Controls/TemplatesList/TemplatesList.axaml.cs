using Avalonia.Controls;

namespace SparkTrack.AvaloniaImpl.Controls.TemplatesList;

using System.Windows.Input;
using Avalonia;
using Avalonia.Data;
using Data.Templates;

public partial class TemplatesList : UserControl
{
    private bool m_changingByTemplate;
    private bool m_changingByGroup;
    
    public TemplatesList()
    {
        InitializeComponent();
    }

    #region GroupsList Property

    public static readonly StyledProperty<IReadOnlyList<TemplateTreeItemProxy>> GroupsListProperty =
        AvaloniaProperty.Register<TemplatesList, IReadOnlyList<TemplateTreeItemProxy>>(nameof(GroupsList));

    public IReadOnlyList<TemplateTreeItemProxy> GroupsList
    {
        get => GetValue(GroupsListProperty);
        set => SetValue(GroupsListProperty, value);
    }

    #endregion

    #region UngroupedTemplatesList Property

    public static readonly StyledProperty<IReadOnlyList<ITemplate>> UngroupedTemplatesListProperty =
        AvaloniaProperty.Register<TemplatesList, IReadOnlyList<ITemplate>>(nameof(UngroupedTemplatesList));

    public IReadOnlyList<ITemplate> UngroupedTemplatesList
    {
        get => GetValue(UngroupedTemplatesListProperty);
        set => SetValue(UngroupedTemplatesListProperty, value);
    }

    #endregion

    #region SelectedTemplate Property

    public static readonly StyledProperty<ITemplate?> SelectedTemplateProperty =
        AvaloniaProperty.Register<TemplatesList, ITemplate?>(nameof(SelectedTemplate), defaultBindingMode: BindingMode.TwoWay);

    public ITemplate? SelectedTemplate
    {
        get => GetValue(SelectedTemplateProperty);
        set => SetValue(SelectedTemplateProperty, value);
    }

    #endregion

    #region SelectedGroup Property

    public static readonly StyledProperty<ITemplateGroup?> SelectedGroupProperty =
        AvaloniaProperty.Register<TemplatesList, ITemplateGroup?>(nameof(SelectedGroup), defaultBindingMode: BindingMode.TwoWay);

    public ITemplateGroup? SelectedGroup
    {
        get => GetValue(SelectedGroupProperty);
        set => SetValue(SelectedGroupProperty, value);
    }

    #endregion

    #region TemplateDoubleTappedCommand Property

    public static readonly StyledProperty<ICommand?> TemplateDoubleTappedCommandProperty =
        AvaloniaProperty.Register<TemplatesList, ICommand?>(nameof(TemplateDoubleTappedCommand));

    public ICommand? TemplateDoubleTappedCommand
    {
        get => GetValue(TemplateDoubleTappedCommandProperty);
        set => SetValue(TemplateDoubleTappedCommandProperty, value);
    }

    #endregion

    #region RemoveCommand Property

    public static readonly StyledProperty<ICommand> RemoveCommandProperty =
        AvaloniaProperty.Register<TemplatesList, ICommand>(nameof(RemoveCommand));

    public ICommand RemoveCommand
    {
        get => GetValue(RemoveCommandProperty);
        set => SetValue(RemoveCommandProperty, value);
    }

    #endregion

    private void GroupsTreeView_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if(m_changingByTemplate) return;
        var selectedGroup = GroupsTreeView.SelectedItem as TemplateTreeItemProxy;

        m_changingByGroup = true;
        
        SelectedTemplate = selectedGroup?.Template;
        SelectedGroup = selectedGroup?.Group;

        UngroupedListBox.SelectedItem = null;
        
        m_changingByGroup = false;
    }

    private void UngroupedListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if(m_changingByGroup) return;

        m_changingByTemplate = true;
        
        SelectedTemplate = UngroupedListBox.SelectedItem as ITemplate;
        GroupsTreeView.SelectedItem = null;
        SelectedGroup = null;

        m_changingByTemplate = false;
    }
}