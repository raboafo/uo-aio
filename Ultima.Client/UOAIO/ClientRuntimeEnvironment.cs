using System;
using System.IO;

namespace UOAIO;

internal static class ClientRuntimeEnvironment
{
    private static string _appBaseDirectory = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
    private static string _runtimeDataRoot = _appBaseDirectory;

    public static string AppBaseDirectory => _appBaseDirectory;

    public static string RuntimeDataRoot => _runtimeDataRoot;

    public static void Initialize(string appBaseDirectory, string runtimeDataRoot)
    {
        if (string.IsNullOrWhiteSpace(appBaseDirectory))
        {
            throw new ArgumentException("An application base directory is required.", nameof(appBaseDirectory));
        }

        if (string.IsNullOrWhiteSpace(runtimeDataRoot))
        {
            throw new ArgumentException("A runtime data root is required.", nameof(runtimeDataRoot));
        }

        _appBaseDirectory = Path.GetFullPath(appBaseDirectory);
        _runtimeDataRoot = Path.GetFullPath(runtimeDataRoot);

        Directory.CreateDirectory(_runtimeDataRoot);
    }

    public static string AppPath(string relativePath)
    {
        return CombineUnderRoot(_appBaseDirectory, relativePath);
    }

    public static string RuntimeDataPath(string relativePath)
    {
        return CombineUnderRoot(_runtimeDataRoot, relativePath);
    }

    public static void EnsureRuntimeDirectory(string relativePath)
    {
        Directory.CreateDirectory(RuntimeDataPath(relativePath));
    }

    public static StreamWriter CreateRuntimeTextWriter(string relativePath, bool append)
    {
        string path = RuntimeDataPath(relativePath);
        string directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return new StreamWriter(path, append);
    }

    private static string CombineUnderRoot(string rootPath, string relativePath)
    {
        string normalizedRelativePath = NormalizeRelativePath(relativePath);
        return normalizedRelativePath.Length == 0
            ? rootPath
            : Path.Combine(rootPath, normalizedRelativePath);
    }

    private static string NormalizeRelativePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return string.Empty;
        }

        return relativePath
            .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
            .TrimStart(Path.DirectorySeparatorChar);
    }
}
