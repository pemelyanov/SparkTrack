using System.Reflection;

namespace Avalonia.SpellChecker
{
    /// <summary>
    /// SpellChecker settings
    /// TODO: Enable/Disable spell checking
    /// </summary>
    public class SpellCheckerConfig
    {
        private static SpellCheckerConfig? _instance;
        
        public string DictionariesFolder { get; set; } = "Dictionaries";
        public string? CustomDictionariesFolder { get; set; } = null;
        public List<string> EnabledLanguages { get; set; } = new List<string>();
    
        public static SpellCheckerConfig Instance => 
            _instance ?? throw new InvalidOperationException(
                "SpellCheckerConfig not initialized. Call SpellCheckerConfig.Initialize() first.");

        public static void Initialize(SpellCheckerConfig config)
        {
            _instance = config;
        }

        public static SpellCheckerConfig Create(params string[] languages)
        {
            var config = new SpellCheckerConfig
            {
                DictionariesFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Dictionaries")
            };

            config.EnabledLanguages.AddRange(languages);

            return config;
        }

    }
}
