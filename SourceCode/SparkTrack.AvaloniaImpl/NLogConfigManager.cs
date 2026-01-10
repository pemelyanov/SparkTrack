namespace SparkTrack.AvaloniaImpl;

using System.Reflection;

public class NLogConfigManager
{
    private static readonly string s_appDataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SparkTrack"
    );

    public static string LogsFolder { get; } = Path.Combine(s_appDataFolder, "logs");

    public static string NLogConfigPath { get; } = Path.Combine(s_appDataFolder, "NLog.config");
    
    public static void EnsureNLogConfig(Assembly resourceAssembly, string configFileName)
    {
        if(!Directory.Exists(s_appDataFolder)) Directory.CreateDirectory(s_appDataFolder);
        
        if (!File.Exists(NLogConfigPath))
        {
            ExtractEmbeddedResource(resourceAssembly, configFileName, NLogConfigPath);
        }
    }
    
    private static void ExtractEmbeddedResource(Assembly assembly, string resourceName, string outputPath)
    {
        var resourceStream = assembly.GetManifestResourceStream(resourceName);
        
        if(resourceStream is null) return;

        try
        {
            using FileStream fileStream = File.Create(outputPath);

            resourceStream.CopyTo(fileStream);
        }
        finally
        {
            resourceStream.Dispose();
        }
    }
}