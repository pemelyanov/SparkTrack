namespace SparkTrack.AvaloniaImpl;

using System.Reflection;
using Core.Client;

public class NLogConfigManager
{
    public static string LogsFolder { get; } = Path.Combine(Paths.ApplicationData, "logs");

    public static string NLogConfigPath { get; } = Path.Combine(Paths.ApplicationData, "NLog.config");
    
    public static void EnsureNLogConfig(Assembly resourceAssembly, string configFileName)
    {
        if(!Directory.Exists(Paths.ApplicationData)) Directory.CreateDirectory(Paths.ApplicationData);
        
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