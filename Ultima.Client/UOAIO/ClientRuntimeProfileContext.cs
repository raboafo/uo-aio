using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using UOAIO.ShardRuntime;

namespace UOAIO;

[DataContract]
internal sealed class ActiveCharacterRecord
{
    [DataMember(Name = "name")]
    public string Name { get; set; } = string.Empty;

    [DataMember(Name = "folder")]
    public string Folder { get; set; } = string.Empty;
}

internal sealed class ClientRuntimeProfileContext
{
    private readonly string _runtimeDataRoot;
    private readonly string _shardName;
    private readonly string _serverName;
    private readonly string _accountName;
    private readonly string _shardFolderName;
    private readonly string _accountFolderName;
    private string _activeCharacterName;
    private string _activeCharacterFolderName;

    public ClientRuntimeProfileContext(string runtimeDataRoot, ShardDefinition shard)
    {
        if (string.IsNullOrWhiteSpace(runtimeDataRoot))
        {
            throw new ArgumentException("A runtime data root is required.", nameof(runtimeDataRoot));
        }

        if (shard == null)
        {
            throw new ArgumentNullException(nameof(shard));
        }

        _runtimeDataRoot = Path.GetFullPath(runtimeDataRoot);
        _shardName = string.IsNullOrWhiteSpace(shard.Name) ? shard.Id ?? shard.Host ?? "shard" : shard.Name;
        _serverName = string.IsNullOrWhiteSpace(shard.Host) ? _shardName : shard.Host;
        _accountName = string.IsNullOrWhiteSpace(shard.Account) ? "default-account" : shard.Account;
        _shardFolderName = Slugify(!string.IsNullOrWhiteSpace(shard.Id) ? shard.Id : _shardName, "shard");
        _accountFolderName = Slugify(_accountName, "account");

        Directory.CreateDirectory(AccountRootPath);
        Directory.CreateDirectory(LogsRootPath);
        Directory.CreateDirectory(NetworkLogsRootPath);
        ReloadActiveCharacter();
    }

    public string ShardName => _shardName;

    public string ServerName => _serverName;

    public string AccountName => _accountName;

    public string ProfilesRootPath => Path.Combine(_runtimeDataRoot, "Profiles");

    public string ShardRootPath => Path.Combine(ProfilesRootPath, _shardFolderName);

    public string AccountRootPath => Path.Combine(ShardRootPath, _accountFolderName);

    public string LogsRootPath => Path.Combine(AccountRootPath, "Logs");

    public string NetworkLogsRootPath => Path.Combine(LogsRootPath, "Network");

    public string ActiveCharacterPath => Path.Combine(AccountRootPath, "active-character.json");

    public string ActiveCharacterName => _activeCharacterName;

    public bool HasActiveCharacter => !string.IsNullOrWhiteSpace(_activeCharacterFolderName);

    public string CharacterRootPath => !HasActiveCharacter
        ? null
        : Path.Combine(AccountRootPath, _activeCharacterFolderName!);

    public void ReloadActiveCharacter()
    {
        _activeCharacterName = null;
        _activeCharacterFolderName = null;

        if (!File.Exists(ActiveCharacterPath))
        {
            return;
        }

        try
        {
            using FileStream stream = File.OpenRead(ActiveCharacterPath);
            DataContractJsonSerializer serializer = new(typeof(ActiveCharacterRecord));
            if (serializer.ReadObject(stream) is ActiveCharacterRecord record && !string.IsNullOrWhiteSpace(record.Folder))
            {
                _activeCharacterName = record.Name ?? string.Empty;
                _activeCharacterFolderName = record.Folder;
                Directory.CreateDirectory(Path.Combine(AccountRootPath, _activeCharacterFolderName));
            }
        }
        catch
        {
            _activeCharacterName = null;
            _activeCharacterFolderName = null;
        }
    }

    public void SelectCharacter(string characterName)
    {
        if (string.IsNullOrWhiteSpace(characterName))
        {
            throw new ArgumentException("A character name is required.", nameof(characterName));
        }

        string folderName = ResolveCharacterFolderName(characterName);
        _activeCharacterName = characterName;
        _activeCharacterFolderName = folderName;

        Directory.CreateDirectory(Path.Combine(AccountRootPath, folderName));
        PersistActiveCharacter(new ActiveCharacterRecord
        {
            Name = characterName,
            Folder = folderName
        });
    }

    public bool TryGetCharacterDataPath(string relativePath, out string path)
    {
        path = string.Empty;
        if (!HasActiveCharacter)
        {
            return false;
        }

        path = CombineUnderRoot(CharacterRootPath!, relativePath);
        return true;
    }

    public string GetAccountDataPath(string relativePath)
    {
        return CombineUnderRoot(AccountRootPath, relativePath);
    }

    public string GetCharacterDataPath(string relativePath)
    {
        if (!TryGetCharacterDataPath(relativePath, out string path))
        {
            throw new InvalidOperationException("No active character is available for character-scoped runtime data.");
        }

        return path;
    }

    public string GetLogPath(string relativePath)
    {
        return CombineUnderRoot(LogsRootPath, relativePath);
    }

    public string GetNetworkLogPath(string relativePath)
    {
        return CombineUnderRoot(NetworkLogsRootPath, relativePath);
    }

    private void PersistActiveCharacter(ActiveCharacterRecord record)
    {
        string directory = Path.GetDirectoryName(ActiveCharacterPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using FileStream stream = new(ActiveCharacterPath, FileMode.Create, FileAccess.Write, FileShare.None);
        DataContractJsonSerializer serializer = new(typeof(ActiveCharacterRecord));
        serializer.WriteObject(stream, record);
    }

    private string ResolveCharacterFolderName(string characterName)
    {
        if (!string.IsNullOrWhiteSpace(_activeCharacterName) &&
            string.Equals(_activeCharacterName, characterName, StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(_activeCharacterFolderName))
        {
            return _activeCharacterFolderName!;
        }

        string baseFolder = Slugify(characterName, "character");
        string suffix = ComputeShortHash(characterName);
        return $"{baseFolder}-{suffix}";
    }

    private static string ComputeShortHash(string value)
    {
        using SHA1 sha1 = SHA1.Create();
        byte[] data = sha1.ComputeHash(Encoding.UTF8.GetBytes(value));
        StringBuilder builder = new StringBuilder(8);
        for (int i = 0; i < 4; i++)
        {
            builder.Append(data[i].ToString("x2"));
        }

        return builder.ToString();
    }

    private static string Slugify(string value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        StringBuilder builder = new();
        foreach (char character in value.Trim())
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToLowerInvariant(character));
            }
            else if (character == '-' || character == '_' || character == ' ')
            {
                if (builder.Length == 0 || builder[builder.Length - 1] == '-')
                {
                    continue;
                }

                builder.Append('-');
            }
        }

        string slug = builder.ToString().Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? fallback : slug;
    }

    private static string CombineUnderRoot(string rootPath, string relativePath)
    {
        string normalizedRelativePath = relativePath
            .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
            .TrimStart(Path.DirectorySeparatorChar);

        return string.IsNullOrWhiteSpace(normalizedRelativePath)
            ? rootPath
            : Path.Combine(rootPath, normalizedRelativePath);
    }
}
