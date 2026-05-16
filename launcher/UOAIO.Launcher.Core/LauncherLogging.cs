using System.Text;

namespace UOAIO.Launcher.Core;

public static class LauncherLogging
{
    private static readonly object SyncRoot = new();
    private static string? _logFilePath;

    public static void Initialize(LauncherPaths paths)
    {
        if (paths is null)
        {
            throw new ArgumentNullException(nameof(paths));
        }

        Initialize(paths.LogFilePath);
    }

    public static void Initialize(string logFilePath)
    {
        if (string.IsNullOrWhiteSpace(logFilePath))
        {
            throw new ArgumentException("A launcher log file path is required.", nameof(logFilePath));
        }

        string fullPath = Path.GetFullPath(logFilePath);
        string? directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        lock (SyncRoot)
        {
            _logFilePath = fullPath;
        }
    }

    public static void Info(string message)
    {
        Write("INFO", message);
    }

    public static void Error(string message, Exception? exception = null)
    {
        StringBuilder builder = new();
        builder.Append(message);
        if (exception is not null)
        {
            builder.AppendLine();
            builder.Append(exception);
        }

        Write("ERROR", builder.ToString());
    }

    private static void Write(string level, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        string? logFilePath;
        lock (SyncRoot)
        {
            logFilePath = _logFilePath;
        }

        if (string.IsNullOrWhiteSpace(logFilePath))
        {
            return;
        }

        try
        {
            string entry = $"[{DateTimeOffset.UtcNow:O}] [{level}] {message}{Environment.NewLine}";
            File.AppendAllText(logFilePath, entry);
        }
        catch
        {
        }
    }
}
