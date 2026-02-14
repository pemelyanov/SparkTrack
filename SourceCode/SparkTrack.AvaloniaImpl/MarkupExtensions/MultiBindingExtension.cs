namespace SparkTrack.AvaloniaImpl.MarkupExtensions;

using Avalonia.Data;
using Avalonia.Data.Converters;

public class MultiBindingExtension
{
    private readonly IList<IBinding> m_bindings;

    public MultiBindingExtension(IBinding binding1, IBinding binding2)
    {
        m_bindings = [binding1, binding2];
    }
    
    public MultiBindingExtension(IBinding binding1, IBinding binding2, IBinding binding3)
    {
        m_bindings = [binding1, binding2, binding3];
    }
    
    public MultiBindingExtension(IBinding binding1, IBinding binding2, IBinding binding3, IBinding binding4)
    {
        m_bindings = [binding1, binding2, binding3, binding4];
    }
    
    public IMultiValueConverter? Converter { get; set; }

    public object ProvideValue()
    {
        return new MultiBinding
        {
            Converter = Converter,
            Bindings = m_bindings
        };
    }
}