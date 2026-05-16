using System;
using System.IO;
using UOAIO.Profiles;
using UOAIO.ShardRuntime;

namespace UOAIO;

internal static class ClientRuntimeEnvironment
{
    private static string _appBaseDirectory = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
    private static string _runtimeDataRoot = _appBaseDirectory;
    private static ClientRuntimeProfileContext _profileContext;

    public static string AppBaseDirectory => _appBaseDirectory;

    public static string RuntimeDataRoot => _runtimeDataRoot;

    public static string ActiveProfileName => string.IsNullOrWhiteSpace(_profileContext?.ActiveCharacterName)
        ? "Default"
        : _profileContext!.ActiveCharacterName!;

    public static string ServerName => _profileContext == null ? null : _profileContext.ServerName;

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
        _profileContext = null;

        Directory.CreateDirectory(_runtimeDataRoot);
    }

    public static void SetShardContext(ShardDefinition shard)
    {
        _profileContext = new ClientRuntimeProfileContext(_runtimeDataRoot, shard);
    }

    public static void SelectCharacter(string characterName)
    {
        if (_profileContext == null)
        {
            throw new InvalidOperationException("Shard runtime context has not been initialized.");
        }

        _profileContext.SelectCharacter(characterName);
        ReloadProfileScopedState();
    }

    public static void ReloadProfileScopedState()
    {
        _profileContext?.ReloadActiveCharacter();
        UOAIO.Profiles.Config.Current.Load();
        Profile.InvalidateCurrent();
        Player.InvalidateCurrent();
        Macros.Reset();
    }

    public static string AppPath(string relativePath)
    {
        return CombineUnderRoot(_appBaseDirectory, relativePath);
    }

    public static string RuntimeDataPath(string relativePath)
    {
        if (_profileContext == null)
        {
            return CombineUnderRoot(_runtimeDataRoot, relativePath);
        }

        string normalizedRelativePath = NormalizeRelativePath(relativePath);
        if (normalizedRelativePath.Length == 0)
        {
            return _profileContext.CharacterRootPath ?? _profileContext.AccountRootPath;
        }

        if (_profileContext.TryGetCharacterDataPath(normalizedRelativePath, out string characterPath))
        {
            return characterPath;
        }

        return _profileContext.GetAccountDataPath(normalizedRelativePath);
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

    public static StreamWriter CreateLogTextWriter(string fileName, bool append, bool network = false)
    {
        string path = GetLogPath(fileName, network);
        string directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return new StreamWriter(path, append);
    }

    public static FileStream CreateUniqueLogFileStream(string baseFileName, string extension, bool network = false)
    {
        string path = GetLogPath(baseFileName + extension, network);
        int suffix = 0;
        do
        {
            try
            {
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                return new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
            }
            catch
            {
                suffix++;
                path = GetLogPath(baseFileName + suffix + extension, network);
            }
        }
        while (suffix < 1000);

        throw new Exception($"Unable to create unique log file (baseFileName='{baseFileName}', extension='{extension}').");
    }

    public static bool TryCharacterDataPath(string relativePath, out string path)
    {
        if (_profileContext == null)
        {
            path = string.Empty;
            return false;
        }

        return _profileContext.TryGetCharacterDataPath(relativePath, out path);
    }

    public static string CharacterDataPath(string relativePath)
    {
        if (_profileContext == null)
        {
            throw new InvalidOperationException("Shard runtime context has not been initialized.");
        }

        return _profileContext.GetCharacterDataPath(relativePath);
    }

    public static string AccountDataPath(string relativePath)
    {
        if (_profileContext == null)
        {
            return CombineUnderRoot(_runtimeDataRoot, relativePath);
        }

        return _profileContext.GetAccountDataPath(relativePath);
    }

    public static string LogPath(string relativePath)
    {
        return GetLogPath(relativePath, network: false);
    }

    public static string NetworkLogPath(string relativePath)
    {
        return GetLogPath(relativePath, network: true);
    }

    private static string GetLogPath(string relativePath, bool network)
    {
        string normalizedRelativePath = NormalizeRelativePath(relativePath);
        if (_profileContext == null)
        {
            string logRoot = network
                ? Path.Combine("Logs", "Network")
                : "Logs";
            return CombineUnderRoot(_runtimeDataRoot, Path.Combine(logRoot, normalizedRelativePath));
        }

        return network
            ? _profileContext.GetNetworkLogPath(normalizedRelativePath)
            : _profileContext.GetLogPath(normalizedRelativePath);
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
