#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace UOAIO.ShardRuntime;

public sealed class ShardCatalog
{
    public string SchemaVersion { get; set; } = "1.0";

    public DateTimeOffset PublishedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public List<ShardDefinition> Shards { get; set; } = new List<ShardDefinition>();
}

public sealed class ShardDefinition
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Host { get; set; } = string.Empty;

    public string Account { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public Version? UOClientVersion { get; set; }

    public IPAddress? ServerIP { get; set; }

    public int ServerPort { get; set; }

    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public string GetMetadata(string key)
    {
        return GetMetadata(key, true);
    }

    public string GetMetadata(string key, bool required)
    {
        bool exists = Metadata.TryGetValue(key, out string value);
        if (required && (!exists || string.IsNullOrWhiteSpace(value)))
        {
            throw new InvalidOperationException("Runtime metadata is missing '" + key + "'.");
        }

        return value;
    }
    public override string ToString()
    {
        return Name;
    }
}

public sealed class ActiveShardState
{
    public int SchemaVersion { get; set; } = 3;

    public string SelectedShardId { get; set; } = string.Empty;

    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public ShardDefinition Runtime { get; set; } = new ShardDefinition();
}

public sealed class ClientBootstrapDefinition
{
    public int SchemaVersion { get; set; } = 2;

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public ShardDefinition Shard { get; set; } = new ShardDefinition();
}

public sealed class ValidationResult
{
    public static ValidationResult Success => new ValidationResult(true, Array.Empty<string>());

    public ValidationResult(bool isValid, IEnumerable<string> errors)
    {
        IsValid = isValid;
        Errors = errors.ToArray();
    }

    public bool IsValid { get; }

    public IReadOnlyList<string> Errors { get; }

    public static ValidationResult Failure(params string[] errors)
    {
        return new ValidationResult(false, errors ?? Array.Empty<string>());
    }
}

public interface IClientShardHandler
{
    string ShardId { get; }
}
