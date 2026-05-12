using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.InteropServices;
using UOAIO.ShardRuntime;

namespace UOAIO.Launcher.Core;

public static class LauncherDefaults
{
    public const string ProductCode = "uoaio";

    public const string ProductionLicenseServerUrl = "https://licenses.example.invalid";

    public const string DeveloperDefaultLicenseServerUrl = "http://localhost:8080";

    public const string LicenseSigningKeyId = "placeholder-launcher-license-v1";

    public const string PlaceholderLicenseSigningPublicKeyPem =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBCgKCAQEA5wUQgVIX8MO/Wkli/GNN3X+fc4zJOnspQVJjIPmO9Z8ytmyui/1j
grXXVd9sDDnPm2PGlOAUP0BK4xCdPmknCLCP+KemnZXoQGYfzbTeujlaaggzNKpB
OPiJDwqZf9Rms0gTGYWmBcaTDfg5tKHi8Lk8eDefhOdQn0L3vS39GCbxmsNPMzru
OBdKw8QeU+9cvixAl8G3GWwiYX+p96jlTPccKB6aFuAszwsbogd+vZa5nBlC3c2k
Q2hAN46uMj8LAkIt9sGRoIS8nUGc7iuy1DZ6jyNld44WIFv71npQfUlv0cKxehiM
5BN/YxF+CmX5tcQfJ3oLVoHd0kSdkfC1IQIDAQAB
-----END RSA PUBLIC KEY-----";

    public static bool AllowDeveloperLicenseServerOverride
    {
        get
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}

public sealed class LauncherPaths
{
    public string DataRoot { get; init; } = string.Empty;

    public string StateFilePath { get; init; } = string.Empty;

    public string ActiveShardStateFilePath { get; init; } = string.Empty;

    public string ShardStateDirectory { get; init; } = string.Empty;

    public string ShardManifestPath { get; init; } = string.Empty;

    public string ProtectedLicenseSecretsPath { get; init; } = string.Empty;

    public static LauncherPaths CreateDefault(string appBaseDirectory)
    {
        string dataRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UOAIO", "Launcher");
        return new LauncherPaths
        {
            DataRoot = dataRoot,
            StateFilePath = Path.Combine(dataRoot, "launcher-state.json"),
            ActiveShardStateFilePath = Path.Combine(dataRoot, "active-shard-state.json"),
            ShardStateDirectory = Path.Combine(dataRoot, "shards"),
            ShardManifestPath = Path.Combine(appBaseDirectory, "Data", "shards.json"),
            ProtectedLicenseSecretsPath = Path.Combine(dataRoot, "license-secrets.bin")
        };
    }
}

public sealed class LicensePolicy
{
    public int MaxDevices { get; set; }

    public int HeartbeatMinutes { get; set; }

    public int GraceHours { get; set; }

    public int ResetAllowance { get; set; }

    public int ResetsUsed { get; set; }

    [JsonPropertyName("offlineTokenAllow")]
    public bool OfflineTokenAllowed { get; set; }
}

public sealed class LauncherState
{
    public int SchemaVersion { get; set; } = 2;

    public string DeveloperLicenseServerUrlOverride { get; set; } = string.Empty;

    public string SelectedShardId { get; set; } = string.Empty;

    public bool RememberInputs { get; set; } = true;

    public LauncherLicenseSummary? LicenseSummary { get; set; }

    public List<ReplayCacheEntry> ReplayCache { get; set; } = new();
}

public sealed class LauncherStateStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters =
        {
            new IPAddressJsonConverter(),
            new VersionJsonConverter()
        }
    };

    private readonly LauncherPaths _paths;

    public LauncherStateStore(LauncherPaths paths)
    {
        _paths = paths;
    }

    public async Task<LauncherState> LoadAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_paths.DataRoot);
        if (!File.Exists(_paths.StateFilePath))
        {
            return new LauncherState();
        }

        string json = await File.ReadAllTextAsync(_paths.StateFilePath, cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<LauncherState>(json, Options) ?? new LauncherState();
    }

    public Task SaveAsync(LauncherState state, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_paths.DataRoot);
        string json = JsonSerializer.Serialize(state, Options);
        return File.WriteAllTextAsync(_paths.StateFilePath, json, cancellationToken);
    }

    public async Task<ActiveShardState?> LoadActiveShardStateAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_paths.DataRoot);
        if (!File.Exists(_paths.ActiveShardStateFilePath))
        {
            return null;
        }

        string json = await File.ReadAllTextAsync(_paths.ActiveShardStateFilePath, cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<ActiveShardState>(json, Options);
    }

    public Task SaveActiveShardStateAsync(ActiveShardState activeState, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_paths.DataRoot);
        string json = JsonSerializer.Serialize(activeState, Options);
        return File.WriteAllTextAsync(_paths.ActiveShardStateFilePath, json, cancellationToken);
    }
}

public sealed class ShardDefinitionStateStore
{
    private const string NewRenaissanceShardId = "new-renaissance";
    private const string UoNewDawnShardId = "uo-new-dawn";

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly LauncherPaths _paths;

    public ShardDefinitionStateStore(LauncherPaths paths)
    {
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
    }

    public ShardDefinition ApplyRememberedState(ShardDefinition shard)
    {
        if (shard is null)
        {
            throw new ArgumentNullException(nameof(shard));
        }

        ShardDefinition merged = CloneShard(shard);
        PersistedShardState? persisted = LoadPersistedState(shard.Id);
        if (persisted is null)
        {
            return merged;
        }

        merged.Account = persisted.Account ?? string.Empty;
        merged.Password = UnprotectPassword(persisted.PasswordCiphertext);
        foreach ((string key, string value) in persisted.Metadata ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase))
        {
            merged.Metadata[key] = value;
        }

        return merged;
    }

    public async Task SaveAsync(ShardDefinition shard, CancellationToken cancellationToken = default)
    {
        if (shard is null)
        {
            throw new ArgumentNullException(nameof(shard));
        }

        Directory.CreateDirectory(_paths.ShardStateDirectory);
        PersistedShardState persisted = new()
        {
            SchemaVersion = 1,
            ShardId = shard.Id,
            Account = shard.Account ?? string.Empty,
            PasswordCiphertext = ProtectPassword(shard.Password),
            Metadata = GetPersistedMetadata(shard)
        };

        string json = JsonSerializer.Serialize(persisted, Options);
        await File.WriteAllTextAsync(GetPath(shard.Id), json, cancellationToken).ConfigureAwait(false);
    }

    public Task DeleteAsync(string shardId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string path = GetPath(shardId);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }

    public string GetPath(string shardId)
    {
        if (string.IsNullOrWhiteSpace(shardId))
        {
            throw new ArgumentException("A shard id is required.", nameof(shardId));
        }

        return Path.Combine(_paths.ShardStateDirectory, $"{shardId}.json");
    }

    private PersistedShardState? LoadPersistedState(string shardId)
    {
        string path = GetPath(shardId);
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            string json = File.ReadAllText(path);
            PersistedShardState? persisted = JsonSerializer.Deserialize<PersistedShardState>(json, Options);
            if (persisted is null || !string.Equals(persisted.ShardId, shardId, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            persisted.Metadata ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            return persisted;
        }
        catch
        {
            return null;
        }
    }

    private static Dictionary<string, string> GetPersistedMetadata(ShardDefinition shard)
    {
        Dictionary<string, string> metadata = new(StringComparer.OrdinalIgnoreCase);
        if (shard.Metadata is not null &&
            shard.Metadata.TryGetValue(ShardMetadataKeys.ClientAssetPath, out string? assetPath) &&
            !string.IsNullOrWhiteSpace(assetPath))
        {
            metadata[ShardMetadataKeys.ClientAssetPath] = assetPath;
        }

        if (string.Equals(shard.Id, UoNewDawnShardId, StringComparison.OrdinalIgnoreCase))
        {
            if (shard.Metadata is not null &&
                shard.Metadata.TryGetValue("refresh_token", out string? refreshToken) &&
                !string.IsNullOrWhiteSpace(refreshToken))
            {
                metadata["refresh_token"] = refreshToken;
            }
        }

        return metadata;
    }

    private static string ProtectPassword(string? password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return string.Empty;
        }

        byte[] raw = Encoding.UTF8.GetBytes(password);
        byte[] protectedBytes = WindowsDataProtection.Protect(raw);
        return Convert.ToBase64String(protectedBytes);
    }

    private static string UnprotectPassword(string? passwordCiphertext)
    {
        if (string.IsNullOrWhiteSpace(passwordCiphertext))
        {
            return string.Empty;
        }

        byte[] protectedBytes = Convert.FromBase64String(passwordCiphertext);
        byte[] raw = WindowsDataProtection.Unprotect(protectedBytes);
        return Encoding.UTF8.GetString(raw);
    }

    private static ShardDefinition CloneShard(ShardDefinition shard)
    {
        return new ShardDefinition
        {
            Id = shard.Id,
            Name = shard.Name,
            Description = shard.Description,
            Host = shard.Host,
            Account = shard.Account,
            Password = shard.Password,
            ClientVersion = shard.ClientVersion is null ? null! : new Version(shard.GetVersionString()),
            ServerIP = shard.ServerIP,
            ServerPort = shard.ServerPort,
            Metadata = new Dictionary<string, string>(shard.Metadata ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase)
        };
    }
}

public sealed class ShardCatalogService
{
    public async Task<ShardCatalog> LoadAsync(string manifestPath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(manifestPath))
        {
            throw new FileNotFoundException("Shard catalog was not found.", manifestPath);
        }

        string json = await File.ReadAllTextAsync(manifestPath, cancellationToken).ConfigureAwait(false);
        ShardCatalog catalog = ParseCatalog(json);

        string[] duplicateIds = catalog.Shards
            .GroupBy(shard => shard.Id, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateIds.Length > 0)
        {
            throw new InvalidOperationException($"Duplicate shard ids found: {string.Join(", ", duplicateIds)}");
        }

        return catalog;
    }

    private static ShardCatalog ParseCatalog(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;
        ShardCatalog catalog = new()
        {
            SchemaVersion = root.TryGetProperty("schemaVersion", out JsonElement schemaVersion) ? schemaVersion.GetString() ?? "1.0" : "1.0",
            PublishedAtUtc = root.TryGetProperty("publishedAtUtc", out JsonElement publishedAtUtc) && publishedAtUtc.ValueKind == JsonValueKind.String
                ? DateTimeOffset.Parse(publishedAtUtc.GetString()!)
                : DateTimeOffset.UtcNow
        };

        if (!root.TryGetProperty("shards", out JsonElement shards) || shards.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Shard catalog could not be parsed.");
        }

        foreach (JsonElement shardElement in shards.EnumerateArray())
        {
            catalog.Shards.Add(new ShardDefinition
            {
                Id = shardElement.TryGetProperty("id", out JsonElement id) ? id.GetString() ?? string.Empty : string.Empty,
                Name = shardElement.TryGetProperty("name", out JsonElement name) ? name.GetString() ?? string.Empty : string.Empty,
                Description = shardElement.TryGetProperty("description", out JsonElement description) ? description.GetString() ?? string.Empty : string.Empty,
                Host = shardElement.TryGetProperty("host", out JsonElement host) ? host.GetString() ?? string.Empty : string.Empty,
                ClientVersion = ReadRequiredVersion(
                    shardElement,
                    shardElement.TryGetProperty("id", out JsonElement shardId) ? shardId.GetString() ?? string.Empty : string.Empty),
                ServerPort = TryReadPort(shardElement),
                Metadata = ReadMetadata(shardElement)
            });
        }

        return catalog;
    }

    private static Version ReadRequiredVersion(JsonElement shardElement, string shardId)
    {
        JsonElement value;
        if (!TryGetVersionProperty(shardElement, out value))
        {
            throw new InvalidOperationException($"Shard '{shardId}' is missing required 'clientVersion'.");
        }

        string? rawVersion = value.GetString();
        if (string.IsNullOrWhiteSpace(rawVersion) || !Version.TryParse(rawVersion, out Version? parsed))
        {
            throw new InvalidOperationException($"Shard '{shardId}' has invalid 'clientVersion' value '{rawVersion}'.");
        }

        return parsed;
    }

    private static bool TryGetVersionProperty(JsonElement shardElement, out JsonElement value)
    {
        if (shardElement.TryGetProperty("clientVersion", out value) && value.ValueKind == JsonValueKind.String)
        {
            return true;
        }

        value = default;
        return false;
    }

    private static Dictionary<string, string> ReadMetadata(JsonElement shardElement)
    {
        Dictionary<string, string> metadata = new(StringComparer.OrdinalIgnoreCase);
        if (!shardElement.TryGetProperty("metadata", out JsonElement metadataElement) || metadataElement.ValueKind != JsonValueKind.Object)
        {
            return metadata;
        }

        foreach (JsonProperty property in metadataElement.EnumerateObject())
        {
            metadata[property.Name] = property.Value.ValueKind == JsonValueKind.String
                ? property.Value.GetString() ?? string.Empty
                : property.Value.ToString();
        }

        return metadata;
    }

    private static int TryReadPort(JsonElement shardElement)
    {
        if (shardElement.TryGetProperty("port", out JsonElement port) && port.TryGetInt32(out int parsedPort))
        {
            return parsedPort;
        }

        if (shardElement.TryGetProperty("serverPort", out JsonElement serverPort) && serverPort.TryGetInt32(out int parsedServerPort))
        {
            return parsedServerPort;
        }

        return 0;
    }
}

public sealed class LauncherLicenseClient
{
    private readonly HttpClient _httpClient;
    private readonly LicenseTrustSettings _trustSettings;

    public LauncherLicenseClient(HttpClient httpClient, LicenseTrustSettings? trustSettings = null)
    {
        _httpClient = httpClient;
        _trustSettings = trustSettings ?? new LicenseTrustSettings();
    }

    public async Task<LicenseVerificationResult> StartSessionAsync(
        Uri serviceBaseUri,
        string licenseKey,
        string hwidSignal,
        string machineTag,
        List<ReplayCacheEntry> replayCache,
        TrustedTimeSnapshot? priorTrustedTimeSnapshot,
        CancellationToken cancellationToken = default)
    {
        Uri requestUri = new Uri(serviceBaseUri, "/internal/v1/launcher/session/start");
        string clientNonce = CreateNonce();
        SignedLicenseRequest request = new()
        {
            LicenseKey = licenseKey.Trim(),
            ProductCode = LauncherDefaults.ProductCode,
            HWIDSignal = hwidSignal,
            MachineTag = machineTag,
            UserAgent = "UOAIO Launcher/2.0",
            ClientNonce = clientNonce
        };

        SignedSessionPayload payload = await PostAndVerifyPayloadAsync<SignedSessionPayload>(requestUri, request, priorTrustedTimeSnapshot, cancellationToken).ConfigureAwait(false);
        ValidatePayload(payload, clientNonce, hwidSignal, replayCache);

        return new LicenseVerificationResult
        {
            LicenseKey = licenseKey.Trim(),
            ServiceBaseUri = serviceBaseUri,
            SessionToken = payload.SessionToken,
            MachineBinding = hwidSignal,
            ResponseId = payload.ResponseId,
            ServerTimeUtc = payload.ServerTimeUtc,
            Summary = new LauncherLicenseSummary
            {
                LicenseId = payload.LicenseId,
                ProductCode = payload.ProductCode,
                Status = payload.Status,
                ExpiresAtUtc = payload.ExpiresAtUtc,
                LastValidatedAtUtc = payload.ServerTimeUtc,
                IsOfflineMode = false,
                Policy = ClonePolicy(payload.Policy)
            },
            TrustedTimeSnapshot = new TrustedTimeSnapshot
            {
                TrustedUtc = payload.ServerTimeUtc,
                TickCount64 = Environment.TickCount64,
                LastResponseId = payload.ResponseId
            }
        };
    }

    public async Task<LicenseVerificationResult> HeartbeatAsync(
        Uri serviceBaseUri,
        string licenseKey,
        string sessionToken,
        string machineBinding,
        List<ReplayCacheEntry> replayCache,
        TrustedTimeSnapshot? priorTrustedTimeSnapshot,
        CancellationToken cancellationToken = default)
    {
        Uri requestUri = new Uri(serviceBaseUri, "/internal/v1/machines/heartbeat");
        string clientNonce = CreateNonce();
        HeartbeatRequest request = new()
        {
            SessionToken = sessionToken,
            ClientNonce = clientNonce
        };

        SignedSessionPayload payload = await PostAndVerifyPayloadAsync<SignedSessionPayload>(requestUri, request, priorTrustedTimeSnapshot, cancellationToken).ConfigureAwait(false);
        ValidatePayload(payload, clientNonce, machineBinding, replayCache);

        return new LicenseVerificationResult
        {
            LicenseKey = licenseKey.Trim(),
            ServiceBaseUri = serviceBaseUri,
            SessionToken = sessionToken,
            MachineBinding = machineBinding,
            ResponseId = payload.ResponseId,
            ServerTimeUtc = payload.ServerTimeUtc,
            Summary = new LauncherLicenseSummary
            {
                LicenseId = payload.LicenseId,
                ProductCode = payload.ProductCode,
                Status = payload.Status,
                ExpiresAtUtc = payload.ExpiresAtUtc,
                LastValidatedAtUtc = payload.ServerTimeUtc,
                IsOfflineMode = false,
                Policy = ClonePolicy(payload.Policy)
            },
            TrustedTimeSnapshot = new TrustedTimeSnapshot
            {
                TrustedUtc = payload.ServerTimeUtc,
                TickCount64 = Environment.TickCount64,
                LastResponseId = payload.ResponseId
            }
        };
    }

    public async Task<OfflineLicenseGrant> IssueOfflineGrantAsync(
        Uri serviceBaseUri,
        string sessionToken,
        string machineBinding,
        List<ReplayCacheEntry> replayCache,
        TrustedTimeSnapshot? priorTrustedTimeSnapshot,
        CancellationToken cancellationToken = default)
    {
        Uri requestUri = new Uri(serviceBaseUri, "/internal/v1/offline-tokens/issue");
        string clientNonce = CreateNonce();
        OfflineGrantRequest request = new()
        {
            SessionToken = sessionToken,
            ClientNonce = clientNonce
        };

        SignedOfflineGrantPayload payload = await PostAndVerifyPayloadAsync<SignedOfflineGrantPayload>(requestUri, request, priorTrustedTimeSnapshot, cancellationToken).ConfigureAwait(false);
        ValidatePayload(payload, clientNonce, machineBinding, replayCache);

        return new OfflineLicenseGrant
        {
            ResponseId = payload.ResponseId,
            OfflineToken = payload.OfflineToken,
            LicenseId = payload.LicenseId,
            ProductCode = payload.ProductCode,
            Status = payload.Status,
            MachineBinding = payload.MachineBinding,
            SessionTokenId = payload.SessionTokenId,
            ClientNonce = payload.ClientNonce,
            ServerNonce = payload.ServerNonce,
            IssuedAtUtc = payload.IssuedAtUtc,
            NotBeforeUtc = payload.NotBeforeUtc,
            ExpiresAtUtc = payload.ExpiresAtUtc,
            ServerTimeUtc = payload.ServerTimeUtc,
            Policy = ClonePolicy(payload.Policy)
        };
    }

    private async Task<TPayload> PostAndVerifyPayloadAsync<TPayload>(
        Uri requestUri,
        object request,
        TrustedTimeSnapshot? priorTrustedTimeSnapshot,
        CancellationToken cancellationToken)
    {
        using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(requestUri, request, cancellationToken).ConfigureAwait(false);
        string rawResponse = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            LauncherErrorResponse? error = string.IsNullOrWhiteSpace(rawResponse)
                ? null
                : JsonSerializer.Deserialize<LauncherErrorResponse>(rawResponse, LauncherLicenseJson.SerializerOptions);
            throw new LicenseValidationException(error?.Error ?? $"License validation failed with status {(int)response.StatusCode}.", response.StatusCode);
        }

        SignedResponseEnvelopeDto envelope = JsonSerializer.Deserialize<SignedResponseEnvelopeDto>(rawResponse, LauncherLicenseJson.SerializerOptions)
            ?? throw new InvalidOperationException("License validation response was empty.");

        if (!string.Equals(envelope.Algorithm, "RS256", StringComparison.Ordinal))
        {
            throw new LicenseValidationException($"Unexpected signature algorithm '{envelope.Algorithm}'.", response.StatusCode);
        }

        if (!string.IsNullOrWhiteSpace(_trustSettings.KeyId) &&
            !string.Equals(envelope.KeyId, _trustSettings.KeyId, StringComparison.Ordinal))
        {
            throw new LicenseValidationException($"Unexpected license signing key id '{envelope.KeyId}'.", response.StatusCode);
        }

        string payloadJson = envelope.Payload.GetRawText();
        VerifyPayloadSignature(payloadJson, envelope.SignatureBase64);
        TPayload payload = JsonSerializer.Deserialize<TPayload>(payloadJson, LauncherLicenseJson.SerializerOptions)
            ?? throw new InvalidOperationException("Signed license payload could not be parsed.");

        DateTimeOffset serverTimeUtc = payload switch
        {
            SignedSessionPayload sessionPayload => sessionPayload.ServerTimeUtc,
            SignedOfflineGrantPayload offlinePayload => offlinePayload.ServerTimeUtc,
            _ => DateTimeOffset.MinValue
        };

        LauncherLicenseService.ValidateTrustedTimeWindow(priorTrustedTimeSnapshot, serverTimeUtc);
        return payload;
    }

    private void VerifyPayloadSignature(string payloadJson, string signatureBase64)
    {
        using RSA rsa = RSA.Create();
        rsa.ImportFromPem(_trustSettings.SigningPublicKeyPem);
        byte[] payloadBytes = LauncherLicenseJson.GetPayloadBytes(payloadJson);
        byte[] signature = Convert.FromBase64String(signatureBase64);
        bool isValid = rsa.VerifyData(payloadBytes, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        if (!isValid)
        {
            throw new LicenseValidationException("License signature verification failed.", null);
        }
    }

    private static string CreateNonce()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(16);
        return Convert.ToHexString(bytes);
    }

    private static void ValidatePayload(SignedSessionPayload payload, string expectedClientNonce, string expectedMachineBinding, List<ReplayCacheEntry> replayCache)
    {
        ValidateReplayAndCommonFields(
            payload.ResponseId,
            payload.ClientNonce,
            expectedClientNonce,
            payload.ProductCode,
            payload.MachineBinding,
            expectedMachineBinding,
            payload.NotBeforeUtc,
            payload.ExpiresAtUtc,
            payload.ServerTimeUtc,
            replayCache);

        if (string.IsNullOrWhiteSpace(payload.SessionToken))
        {
            throw new LicenseValidationException("The signed session payload did not include a session token.", null);
        }
    }

    private static void ValidatePayload(SignedOfflineGrantPayload payload, string expectedClientNonce, string expectedMachineBinding, List<ReplayCacheEntry> replayCache)
    {
        ValidateReplayAndCommonFields(
            payload.ResponseId,
            payload.ClientNonce,
            expectedClientNonce,
            payload.ProductCode,
            payload.MachineBinding,
            expectedMachineBinding,
            payload.NotBeforeUtc,
            payload.ExpiresAtUtc,
            payload.ServerTimeUtc,
            replayCache);

        if (string.IsNullOrWhiteSpace(payload.OfflineToken))
        {
            throw new LicenseValidationException("The signed offline grant did not include an offline token.", null);
        }
    }

    private static void ValidateReplayAndCommonFields(
        string responseId,
        string actualClientNonce,
        string expectedClientNonce,
        string productCode,
        string actualMachineBinding,
        string expectedMachineBinding,
        DateTimeOffset notBeforeUtc,
        DateTimeOffset expiresAtUtc,
        DateTimeOffset serverTimeUtc,
        List<ReplayCacheEntry> replayCache)
    {
        if (string.IsNullOrWhiteSpace(responseId))
        {
            throw new LicenseValidationException("The signed response is missing a response identifier.", null);
        }

        if (replayCache.Any(entry => string.Equals(entry.ResponseId, responseId, StringComparison.Ordinal) && entry.ExpiresAtUtc > serverTimeUtc))
        {
            throw new LicenseValidationException("A replayed signed response was rejected.", null);
        }

        if (!string.Equals(actualClientNonce, expectedClientNonce, StringComparison.Ordinal))
        {
            throw new LicenseValidationException("The license response nonce did not match the request nonce.", null);
        }

        if (!string.Equals(productCode, LauncherDefaults.ProductCode, StringComparison.OrdinalIgnoreCase))
        {
            throw new LicenseValidationException($"Unexpected product code '{productCode}'.", null);
        }

        if (!string.Equals(actualMachineBinding, expectedMachineBinding, StringComparison.Ordinal))
        {
            throw new LicenseValidationException("The signed license payload was not bound to this machine.", null);
        }

        if (serverTimeUtc < notBeforeUtc || serverTimeUtc > expiresAtUtc)
        {
            throw new LicenseValidationException("The signed license payload is outside its valid time window.", null);
        }
    }

    private static LicensePolicy ClonePolicy(LicensePolicy policy)
    {
        return new LicensePolicy
        {
            MaxDevices = policy.MaxDevices,
            HeartbeatMinutes = policy.HeartbeatMinutes,
            GraceHours = policy.GraceHours,
            ResetAllowance = policy.ResetAllowance,
            ResetsUsed = policy.ResetsUsed,
            OfflineTokenAllowed = policy.OfflineTokenAllowed
        };
    }
}

public sealed class LicenseValidationException : Exception
{
    public LicenseValidationException(string message, HttpStatusCode? statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode? StatusCode { get; }
}

public static class HardwareFingerprintProvider
{
    public static string CreateSignal()
    {
        string raw = string.Join("|", new[]
        {
            $"machine:{Environment.MachineName}",
            $"user:{Environment.UserName}",
            $"os:{Environment.OSVersion.VersionString}",
            $"cpu:{Environment.ProcessorCount}"
        });

        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return $"sha256:{Convert.ToHexString(bytes)}";
    }
}

public sealed class ShardWorkflowRegistration<TValue>
{
    public ShardWorkflowRegistration(string shardId, TValue value)
    {
        ShardId = shardId;
        Value = value;
    }

    public string ShardId { get; }

    public TValue Value { get; }
}

public sealed class ShardWorkflowRegistry<TValue>
{
    private readonly Dictionary<string, TValue> _items;

    public ShardWorkflowRegistry(IEnumerable<ShardWorkflowRegistration<TValue>> registrations)
    {
        _items = registrations.ToDictionary(registration => registration.ShardId, registration => registration.Value, StringComparer.OrdinalIgnoreCase);
    }

    public TValue Resolve(string shardId)
    {
        if (_items.TryGetValue(shardId, out TValue? value))
        {
            return value;
        }

        throw new InvalidOperationException($"Shard workflow '{shardId}' is not registered.");
    }
}

public static class ShardConnectionResolver
{
    public static async Task<IPAddress> ResolveIpAddressAsync(string host, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IPAddress[] addresses = await Dns.GetHostAddressesAsync(host).ConfigureAwait(false);
        IPAddress? selected = addresses.FirstOrDefault(address => address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            ?? addresses.FirstOrDefault();

        if (selected is null)
        {
            throw new InvalidOperationException($"DNS resolution returned no addresses for '{host}'.");
        }

        return selected;
    }
}

public static class ActiveShardStateFactory
{
    public static ActiveShardState Create(ShardDefinition shard)
    {
        if (shard is null)
        {
            throw new ArgumentNullException(nameof(shard));
        }

        if (string.IsNullOrWhiteSpace(shard.Id))
        {
            throw new InvalidOperationException("Shard id is required.");
        }

        if (string.IsNullOrWhiteSpace(shard.Host))
        {
            throw new InvalidOperationException("Shard host is required.");
        }

        if (shard.ServerIP is null)
        {
            throw new InvalidOperationException("Shard server IP is required.");
        }

        if (shard.ServerPort <= 0)
        {
            throw new InvalidOperationException("Shard port must be a positive integer.");
        }

        if (shard.ClientVersion is null)
        {
            throw new InvalidOperationException("Shard client version is required.");
        }

        return new ActiveShardState
        {
            SchemaVersion = 3,
            SelectedShardId = shard.Id,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            Runtime = CloneRuntimeShard(shard)
        };
    }

    private static ShardDefinition CloneRuntimeShard(ShardDefinition shard)
    {
        return new ShardDefinition
        {
            Id = shard.Id,
            Name = shard.Name,
            Description = shard.Description,
            Host = shard.Host,
            Account = shard.Account,
            Password = shard.Password,
            ClientVersion = new Version(shard.GetVersionString()),
            ServerIP = shard.ServerIP,
            ServerPort = shard.ServerPort,
            Metadata = new Dictionary<string, string>(shard.Metadata ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase)
        };
    }
}

internal sealed class PersistedShardState
{
    public int SchemaVersion { get; set; }

    public string ShardId { get; set; } = string.Empty;

    public string Account { get; set; } = string.Empty;

    public string PasswordCiphertext { get; set; } = string.Empty;

    public Dictionary<string, string> Metadata { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

internal static class WindowsDataProtection
{
    public static byte[] Protect(byte[] plainBytes)
    {
        return CryptProtectOrUnprotect(plainBytes, protect: true);
    }

    public static byte[] Unprotect(byte[] protectedBytes)
    {
        return CryptProtectOrUnprotect(protectedBytes, protect: false);
    }

    private static byte[] CryptProtectOrUnprotect(byte[] inputBytes, bool protect)
    {
        if (inputBytes is null)
        {
            throw new ArgumentNullException(nameof(inputBytes));
        }

        DATA_BLOB input = default;
        DATA_BLOB output = default;
        try
        {
            input = CreateBlob(inputBytes);
            bool success = protect
                ? CryptProtectData(ref input, null, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0, ref output)
                : CryptUnprotectData(ref input, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0, ref output);

            if (!success)
            {
                throw new InvalidOperationException($"DPAPI operation failed with Win32 error {Marshal.GetLastWin32Error()}.");
            }

            byte[] result = new byte[output.cbData];
            Marshal.Copy(output.pbData, result, 0, output.cbData);
            return result;
        }
        finally
        {
            FreeBlob(ref input, freeWithLocalFree: false);
            FreeBlob(ref output, freeWithLocalFree: true);
        }
    }

    private static DATA_BLOB CreateBlob(byte[] bytes)
    {
        DATA_BLOB blob = new()
        {
            cbData = bytes.Length,
            pbData = Marshal.AllocHGlobal(bytes.Length)
        };
        Marshal.Copy(bytes, 0, blob.pbData, bytes.Length);
        return blob;
    }

    private static void FreeBlob(ref DATA_BLOB blob, bool freeWithLocalFree)
    {
        if (blob.pbData == IntPtr.Zero)
        {
            return;
        }

        if (freeWithLocalFree)
        {
            LocalFree(blob.pbData);
        }
        else
        {
            Marshal.FreeHGlobal(blob.pbData);
        }

        blob.pbData = IntPtr.Zero;
        blob.cbData = 0;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DATA_BLOB
    {
        public int cbData;

        public IntPtr pbData;
    }

    [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool CryptProtectData(
        ref DATA_BLOB pDataIn,
        string? szDataDescr,
        IntPtr pOptionalEntropy,
        IntPtr pvReserved,
        IntPtr pPromptStruct,
        int dwFlags,
        ref DATA_BLOB pDataOut);

    [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool CryptUnprotectData(
        ref DATA_BLOB pDataIn,
        IntPtr ppszDataDescr,
        IntPtr pOptionalEntropy,
        IntPtr pvReserved,
        IntPtr pPromptStruct,
        int dwFlags,
        ref DATA_BLOB pDataOut);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr LocalFree(IntPtr hMem);
}

public sealed class IPAddressJsonConverter : JsonConverter<IPAddress?>
{
    public override IPAddress? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? value = reader.GetString();
        return string.IsNullOrWhiteSpace(value) ? null : IPAddress.Parse(value);
    }

    public override void Write(Utf8JsonWriter writer, IPAddress? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value?.ToString());
    }
}

public sealed class VersionJsonConverter : JsonConverter<Version?>
{
    public override Version? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? value = reader.GetString();
        return string.IsNullOrWhiteSpace(value) ? null : Version.Parse(value);
    }

    public override void Write(Utf8JsonWriter writer, Version? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value?.ToString());
    }
}

internal sealed class LauncherSessionStartRequest
{
    public string LicenseKey { get; set; } = string.Empty;

    public string Product { get; set; } = string.Empty;

    public string HWIDSignal { get; set; } = string.Empty;

    public string MachineTag { get; set; } = string.Empty;

    public string UserAgent { get; set; } = string.Empty;
}

internal sealed class LauncherSessionStartResponse
{
    public string SessionToken { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }

    public string LicenseId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public LauncherPolicyResponse Policy { get; set; } = new();
}

internal sealed class LauncherPolicyResponse
{
    public int MaxDevices { get; set; }

    public int HeartbeatMinutes { get; set; }

    public int GraceHours { get; set; }

    public int ResetAllowance { get; set; }

    public int ResetUsed { get; set; }

    public bool OfflineTokenAllow { get; set; }
}

internal sealed class LauncherErrorResponse
{
    public string Error { get; set; } = string.Empty;
}
