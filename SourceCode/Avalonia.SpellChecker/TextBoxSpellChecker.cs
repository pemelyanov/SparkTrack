using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.VisualTree;
using System.Windows.Input;

namespace Avalonia.SpellChecker;

using FluentAvalonia.UI.Controls;

/// <summary>
/// This class applies spell checking to TextBox controls without the need of extending the TextBox class.
/// </summary>
public class TextBoxSpellChecker
{
    private readonly HashSet<TextBox>   controls = new HashSet<TextBox>();
    private readonly SpellCheckerConfig config;

    public TextBoxSpellChecker(SpellCheckerConfig config)
    {
        this.config = config;
    }

    public void Initialize(TextBox textBox)
    {

        if (config is null)
        {
            return;
        }


        if (config.EnabledLanguages is null || config.EnabledLanguages.Count == 0)
        {
            return;
        }

        controls.Add(textBox);

        // Create a new StyleInclude instance
        var styleInclude = new StyleInclude(new Uri("avares://Avalonia.SpellChecker/"))
        {
            Source = new Uri("avares://Avalonia.SpellChecker/Styles/SpellCheckerStyles.axaml")
        };

        // Add the style to the Window's Styles collection
        textBox.Styles.Add(styleInclude);

        // Initialize the SpellCheckerTextPresenter setting
        textBox.TemplateApplied += OnTemplateApplied;

        // Clean up
        textBox.DetachedFromLogicalTree += OnTextBoxDisposed;

        textBox.AddHandler(Control.ContextRequestedEvent, TextBox_ContextRequested, handledEventsToo: true);
    }

    private void OnTemplateApplied(object? sender, Controls.Primitives.TemplateAppliedEventArgs e)
    {
        var textPresenter = e.NameScope.Find<SpellCheckerTextPresenter>("PART_TextPresenter");

        if (textPresenter is null)
        {
            return;
        }

        textPresenter.SpellChecker = new SpellChecker(config);

        if (sender is TextBox textBox)
        {
            textBox.TemplateApplied -= OnTemplateApplied;
        }
    }


    private void OnTextBoxDisposed(object? sender, Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e)
    {
        if (sender is null)
        {
            return;
        }

        if (sender is TextBox textBox)
        {
            textBox.DetachedFromLogicalTree -= OnTextBoxDisposed;
            textBox.RemoveHandler(Control.ContextRequestedEvent, TextBox_ContextRequested);
            controls.Remove(textBox);
        }
    }

    private void TextBox_ContextRequested(object? sender, ContextRequestedEventArgs e)
    {

        if (sender is not TextBox textBox)
        {
            return;
        }

        if (!e.TryGetPosition(textBox, out Point point))
        {
            return;
        }

        var textPresenter = textBox.GetVisualDescendants().OfType<Avalonia.SpellChecker.SpellCheckerTextPresenter>().FirstOrDefault();

        if (textPresenter is null)
        {
            return;
        }

        var suggestions = textPresenter.GetSuggestionsAt(point, out string? mispelledWord);

        IEnumerable<SpellCheckSuggestion> spellCheckSuggestions = suggestions?.ToArray() ?? [];
        if (textBox.ContextFlyout is MenuFlyout contextFlyout)
        {
            if (!string.IsNullOrEmpty(mispelledWord))
            {
                contextFlyout.Items.Insert(0, new MenuItem
                {
                    Header = "Добавить в словарь...",
                    Tag = "spell",
                    Command = new BasicCommand<string, TextBox>(IgnoreWordSelected, textBox),
                    CommandParameter = mispelledWord
                });
                contextFlyout.Items.Insert(1, new MenuItem() { Header = "-", Tag = "spell" });
            }

            if (suggestions is null)
            {
                // No suggestions available
                return;
            }

            // Remove suggestions after the context menu is closed
            contextFlyout.Closed += this.TransientMenuFalyout_Closed;

            int insertIndex = 0;

            foreach (var suggestion in spellCheckSuggestions)
            {
                contextFlyout.Items.Insert(insertIndex++, new MenuItem
                {
                    Header = suggestion.WordSuggested,
                    Tag = "spell",
                    Command = new AcceptSuggestionCommand(SuggestionSelected, textBox, suggestion)
                });
            }

            if (insertIndex > 0)
            {
                contextFlyout.Items.Insert(insertIndex, new MenuItem() { Header = "-", Tag = "spell" });
            }

            contextFlyout.ShowAt(textBox, true);

            e.Handled = true;
        }

        if (textBox.ContextFlyout is TextCommandBarFlyout textCommandBarFlyout)
        {
            textCommandBarFlyout.Opened -= TextCommandBarFlyoutOnOpening;
            textCommandBarFlyout.Opened += TextCommandBarFlyoutOnOpening;

            void TextCommandBarFlyoutOnOpening(object? o, EventArgs eventArgs)
            {
                textCommandBarFlyout.Opened -= TextCommandBarFlyoutOnOpening;
                    
                if (!string.IsNullOrEmpty(mispelledWord))
                {
                    textCommandBarFlyout.SecondaryCommands.Insert(0, new CommandBarButton
                    {
                        Label = "Добавить в словарь...",
                        Tag = "spell",
                        Command = new BasicCommand<string, TextBox>(IgnoreWordSelected, textBox),
                        CommandParameter = mispelledWord
                    });
                }

                if (suggestions is null)
                {
                    // No suggestions available
                    return;
                }

                // Remove suggestions after the context menu is closed
                textCommandBarFlyout.Closed += TransientMenuFalyout_Closed;

                int insertIndex = 0;

                foreach (var suggestion in spellCheckSuggestions)
                {
                    textCommandBarFlyout.SecondaryCommands.Insert(insertIndex++, new CommandBarButton
                    {
                        Label = suggestion.WordSuggested,
                        Tag = "spell",
                        Command = new AcceptSuggestionCommand(SuggestionSelected, textBox, suggestion)
                    });
                }

                textCommandBarFlyout.ShowAt(textBox, true);
            }
        }
    }

    private void TransientMenuFalyout_Closed(object? sender, EventArgs e)
    {
        if (sender is not MenuFlyout menu)
        {
            return;
        }

        menu.Closed -= this.TransientMenuFalyout_Closed;

        for (int i = menu.Items.Count - 1; i >= 0; i--)
        {
            if (menu.Items[i] is MenuItem mi && "spell".Equals(mi.Tag))
            {
                menu.Items.RemoveAt(i);
            }
        }
    }
        
    private void IgnoreWordSelected(string misspelledWord, TextBox textBox)
    {
        var spellChecker = new SpellChecker(config);
        spellChecker.AddCustomWord(misspelledWord);
        textBox
            .GetVisualDescendants()
            .OfType<Avalonia.SpellChecker.SpellCheckerTextPresenter>()
            .FirstOrDefault()?
            .ForceInvalidateTextLayout();
    }

    private void SuggestionSelected(TextBox textBox, SpellCheckSuggestion suggestion)
    {
        textBox.Text = textBox.Text?
            .Remove(suggestion.OriginalWordPosition, suggestion.OriginalWordLength)
            .Insert(suggestion.OriginalWordPosition, suggestion.WordSuggested);
    }


}
// Custom ICommand implementation
public class AcceptSuggestionCommand : ICommand
{
    private readonly Action<TextBox, SpellCheckSuggestion> execute;
    private readonly TextBox                               textBox;
    private readonly SpellCheckSuggestion                  suggestion;

    public AcceptSuggestionCommand(Action<TextBox, SpellCheckSuggestion> execute, TextBox textBox, SpellCheckSuggestion suggestion)
    {
        this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
        this.textBox = textBox;
        this.suggestion = suggestion;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public void Execute(object? parameter)
    {
        execute(this.textBox, this.suggestion);
    }
}

public class BasicCommand<ArgType, ControlType> : ICommand
{
    private readonly Action<ArgType, ControlType> execute;
    private readonly ControlType                  control;

    public BasicCommand(Action<ArgType, ControlType> execute, ControlType control)
    {
        this.control = control;
        this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public void Execute(object? parameter)
    {
        execute((ArgType)parameter, control);
    }
}