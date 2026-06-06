namespace Avalonia.SpellChecker.Behaviors;

using Controls;

public static class SpellChecker
{
    public static readonly AttachedProperty<bool> IsEnabledProperty =
        AvaloniaProperty.RegisterAttached<TextBox, bool>(
            "IsEnabled",
            typeof(SpellChecker));

    private static readonly Dictionary<TextBox, TextBoxSpellChecker> activeCheckers = new();

    static SpellChecker()
    {
        IsEnabledProperty.Changed.AddClassHandler<TextBox>(OnIsEnabledChanged);
    }

    public static bool GetIsEnabled(TextBox element) =>
        element.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(TextBox element, bool value) =>
        element.SetValue(IsEnabledProperty, value);

    private static void OnIsEnabledChanged(TextBox textBox, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is true)
        {
            if (!activeCheckers.ContainsKey(textBox))
            {
                var checker = new TextBoxSpellChecker(SpellCheckerConfig.Instance);
                checker.Initialize(textBox);
                activeCheckers[textBox] = checker;
            }
        }
        else
        {
            // При отключении — чекер уже сам подписан на DetachedFromLogicalTree,
            // но можно вручную убрать из словаря
            activeCheckers.Remove(textBox);
        }
    }
}