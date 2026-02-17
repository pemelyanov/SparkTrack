namespace SparkTrack.WebAPI.DataStore;

using System.Collections.Concurrent;
using Google.Apis.Json;
using Google.Apis.Util.Store;
using NLog;

/// <summary>
/// Thread-safe file data store that implements <see cref="IDataStore"/>. 
/// This store creates a different file for each combination of type and key.
/// This file data store stores a JSON format of the specified object.
/// </summary>
public class ThreadSafeFileDataStore : IDataStore
{
    private static readonly ILogger s_logger = LogManager.GetCurrentClassLogger();
    
    private const string XdgDataHomeSubdirectory = "google-filedatastore";

    // Словарь для хранения блокировок для каждого файла
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> s_fileLocks = new();
    
    // Глобальная блокировка для операций с каталогами
    private static readonly SemaphoreSlim s_directoryLock = new(1, 1);
    
    private readonly string m_folderPath;

    /// <summary>
    /// Constructs a new thread-safe file data store.
    /// </summary>
    /// <param name="folder">Folder path.</param>
    /// <param name="fullPath">
    /// Defines whether the folder parameter is absolute or relative.
    /// </param>
    public ThreadSafeFileDataStore(string? folder)
    {
        m_folderPath = Path.IsPathRooted(folder)
            ? folder ?? throw new ArgumentNullException(nameof(folder))
            : GetFullPath(folder ?? throw new ArgumentNullException(nameof(folder)));
            
        _ = EnsureDirectoryExists(m_folderPath);
    }

    /// <summary>
    /// Stores the given value for the given key in a thread-safe manner.
    /// </summary>
    public async Task StoreAsync<T>(string key, T value)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key MUST have a value");
        }

        var serialized = NewtonsoftJsonSerializer.Instance.Serialize(value);
        var filePath = GetFilePath<T>(key);
        var fileLock = GetFileLock(filePath);
        
        await fileLock.WaitAsync();
        s_logger.Info("Storing key '{key}' with value type '{type}'. File path: {path}", key, typeof(T), filePath);
        try
        {
            // Используем временный файл для атомарной записи
            var tempFilePath = filePath + ".tmp";
            await File.WriteAllTextAsync(tempFilePath, serialized);
            
            // Атомарная замена файла
            File.Move(tempFilePath, filePath, true);
        }
        finally
        {
            fileLock.Release();
        }
    }

    /// <summary>
    /// Deletes the given key in a thread-safe manner.
    /// </summary>
    public async Task DeleteAsync<T>(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key MUST have a value");
        }

        var filePath = GetFilePath<T>(key);
        var fileLock = GetFileLock(filePath);

        await fileLock.WaitAsync();
        try
        {
            s_logger.Info("Deleting key '{key}' with value type '{type}'. File path: {path}", key, typeof(T), filePath);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        finally
        {
            fileLock.Release();
            // Очищаем блокировку если она больше не нужна
            CleanupFileLock(filePath);
        }
    }

    /// <summary>
    /// Returns the stored value for the given key in a thread-safe manner.
    /// </summary>
    public async Task<T?> GetAsync<T>(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key MUST have a value");
        }

        var filePath = GetFilePath<T>(key);
        
        // Для чтения используем shared lock для возможности конкурентного чтения
        if (!File.Exists(filePath))
        {
            return default;
        }

        var fileLock = GetFileLock(filePath);
        await fileLock.WaitAsync();
        try
        {
            if (!File.Exists(filePath))
            {
                return default;
            }

            s_logger.Info("Reading key '{key}' with value type '{type}'. File path: {path}", key, typeof(T), filePath);
            var content = await File.ReadAllTextAsync(filePath);
            return NewtonsoftJsonSerializer.Instance.Deserialize<T>(content);
        }
        catch (Exception ex)
        {
            // Логируем ошибку чтения
            // Можно добавить logger через dependency injection
            throw new IOException($"Failed to read file {filePath}", ex);
        }
        finally
        {
            fileLock.Release();
        }
    }

    /// <summary>
    /// Clears all values in the data store in a thread-safe manner.
    /// </summary>
    public async Task ClearAsync()
    {
        await s_directoryLock.WaitAsync();
        try
        {
            if (Directory.Exists(m_folderPath))
            {
                // Очищаем все блокировки для файлов в этой папке
                var files = Directory.GetFiles(m_folderPath);
                foreach (var file in files)
                {
                    CleanupFileLock(file);
                }

                s_logger.Info("Clearing store with path: {path}", m_folderPath);
                // Удаляем и пересоздаем папку
                Directory.Delete(m_folderPath, true);
                Directory.CreateDirectory(m_folderPath);
            }
        }
        finally
        {
            s_directoryLock.Release();
        }
    }

    #region Private Helper Methods

    private string GetFullPath(string folder)
    {
        string? appData = Environment.GetEnvironmentVariable("APPDATA");
        if (!string.IsNullOrEmpty(appData))
        {
            return Path.Combine(appData, folder);
        }
        
        string? home = Environment.GetEnvironmentVariable("HOME");
        if (!string.IsNullOrEmpty(home))
        {
            string? xdgDataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
            if (string.IsNullOrEmpty(xdgDataHome))
            {
                xdgDataHome = Path.Combine(home, ".local", "share");
            }
            return Path.Combine(xdgDataHome, XdgDataHomeSubdirectory, folder);
        }
        
        throw new PlatformNotSupportedException("Relative FileDataStore paths not supported on this platform.");
    }

    private async Task EnsureDirectoryExists(string path)
    {
       
        if (!Directory.Exists(path))
        {
            try
            {
                 await s_directoryLock.WaitAsync();
                // Double-check pattern
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            finally
            {
                s_directoryLock.Release();
            }
        }
    }

    private string GetFilePath<T>(string key)
    {
        var fileName = GenerateStoredKey(key, typeof(T));
        return Path.Combine(m_folderPath, fileName);
    }

    private SemaphoreSlim GetFileLock(string filePath)
    {
        return s_fileLocks.GetOrAdd(filePath, _ => new SemaphoreSlim(1, 1));
    }

    private void CleanupFileLock(string filePath)
    {
        if (s_fileLocks.TryRemove(filePath, out var semaphore))
        {
            try
            {
                semaphore.Dispose();
            }
            catch
            {
                // Игнорируем ошибки при очистке
            }
        }
    }

    /// <summary>Creates a unique stored key based on the key and the class type.</summary>
    public static string GenerateStoredKey(string key, Type t)
    {
        return $"{t.FullName}-{key}";
    }

    #endregion
    
    #region IDisposable Implementation
    
    private bool m_disposed;
    
    protected virtual void Dispose(bool disposing)
    {
        if (!m_disposed)
        {
            if (disposing)
            {
                // Очищаем все блокировки
                foreach (var semaphore in s_fileLocks.Values)
                {
                    try
                    {
                        semaphore.Dispose();
                    }
                    catch
                    {
                        // Игнорируем ошибки при очистке
                    }
                }
                s_fileLocks.Clear();
                s_directoryLock.Dispose();
            }
            m_disposed = true;
        }
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    ~ThreadSafeFileDataStore()
    {
        Dispose(false);
    }
    
    #endregion
}