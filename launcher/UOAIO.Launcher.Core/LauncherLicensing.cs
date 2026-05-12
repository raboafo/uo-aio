using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace UOAIO.Launcher.Core;

public enum LicenseAccessMode
{
    Unlicensed,
    Licensed,
    Offline
}

public sealed class LicenseTrustSettings
{
    public string ProductionLicenseServerUrl { get; init; } = LauncherDefaults.ProductionLicenseServerUrl;

    public string SigningPublicKeyPem { get; init; } = LauncherDefaults.PlaceholderLicenseSigningPublicKeyPem;

    public string KeyId { get; init; } = LauncherDefaults.LicenseSigningKeyId;

    public bool AllowDeveloperOverride { get; init; } = LauncherDefaults.AllowDeveloperLicenseServerOverride;

    public Uri ResolveServiceBaseUri(string? developerOverrideUrl)
    {
        string rawUrl = string.IsNullOrWhiteSpace(developerOverrideUrl) || !AllowDeveloperOverride
            ? ProductionLicenseServerUrl
            : developerOverrideUrl.Trim();

        if (!Uri.TryCreate(rawUrl, UriKind.Absolute, out Uri? uri))
        {
            throw new InvalidOperationException($"Invalid license server URL '{rawUrl}'.");
        }

        if (string.IsNullOrWhiteSpace(developerOverrideUrl) || !AllowDeveloperOverride)
        {
            if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("The production license endpoint must use HTTPS.");
            }
        }

        return uri;
    }
}

public sealed class LauncherLicenseSummary
{
    public string LicenseId { get; set; } = string.Empty;

    public string ProductCode { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAtUtc { get; set; }

    public DateTimeOffset LastValidatedAtUtc { get; set; }

    public bool IsOfflineMode { get; set; }

    public LicensePolicy Policy { get; set; } = new();
}

public sealed class ReplayCacheEntry
{
    public string ResponseId { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAtUtc { get; set; }
}

public sealed class TrustedTimeSnapshot
{
    public DateTimeOffset TrustedUtc { get; set; }

    public long TickCount64 { get; set; }

    public string LastResponseId { get; set; } = string.Empty;
}

public sealed class SignedLicenseEnvelope
{
    public string KeyId { get; set; } = string.Empty;

    public string Algorithm { get; set; } = string.Empty;

    public string PayloadJson { get; set; } = string.Empty;

    public string SignatureBase64 { get; set; } = string.Empty;
}

public sealed class LicenseVerificationResult
{
    public string LicenseKey { get; set; } = string.Empty;

    public Uri ServiceBaseUri { get; set; } = new("https://licenses.example.invalid");

    public string SessionToken { get; set; } = string.Empty;

    public string MachineBinding { get; set; } = string.Empty;

    public string ResponseId { get; set; } = string.Empty;

    public DateTimeOffset ServerTimeUtc { get; set; }

    public LauncherLicenseSummary Summary { get; set; } = new();

    public TrustedTimeSnapshot TrustedTimeSnapshot { get; set; } = new();
}

public sealed class OfflineLicenseGrant
{
    public string ResponseId { get; set; } = string.Empty;

    public string OfflineToken { get; set; } = string.Empty;

    public string LicenseId { get; set; } = string.Empty;

    public string ProductCode { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string MachineBinding { get; set; } = string.Empty;

    public string SessionTokenId { get; set; } = string.Empty;

    public string ClientNonce { get; set; } = string.Empty;

    public string ServerNonce { get; set; } = string.Empty;

    public DateTimeOffset IssuedAtUtc { get; set; }

    public DateTimeOffset NotBeforeUtc { get; set; }

    public DateTimeOffset ExpiresAtUtc { get; set; }

    public DateTimeOffset ServerTimeUtc { get; set; }

    public LicensePolicy Policy { get; set; } = new();
}

public sealed class ProtectedLicenseSecrets
{
    public string LicenseKey { get; set; } = string.Empty;

    public string SessionToken { get; set; } = string.Empty;

    public string MachineBinding { get; set; } = string.Empty;

    public TrustedTimeSnapshot? TrustedTimeSnapshot { get; set; }

    public OfflineLicenseGrant? OfflineGrant { get; set; }
}

public sealed class LicenseGateState
{
    public LicenseAccessMode AccessMode { get; set; }

    public string StatusMessage { get; set; } = string.Empty;

    public string RecoveredLicenseKey { get; set; } = string.Empty;

    public string EffectiveServerUrl { get; set; } = string.Empty;

    public LauncherLicenseSummary? Summary { get; set; }

    public bool CanEnterLauncher => AccessMode == LicenseAccessMode.Licensed || AccessMode == LicenseAccessMode.Offline;
}

public sealed class ProtectedLicenseSecretStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private readonly LauncherPaths _paths;

    public ProtectedLicenseSecretStore(LauncherPaths paths)
    {
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
    }

    public async Task<ProtectedLicenseSecrets?> LoadAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_paths.DataRoot);
        if (!File.Exists(_paths.ProtectedLicenseSecretsPath))
        {
            return null;
        }

        byte[] protectedBytes = await File.ReadAllBytesAsync(_paths.ProtectedLicenseSecretsPath, cancellationToken).ConfigureAwait(false);
        byte[] rawBytes = WindowsDataProtection.Unprotect(protectedBytes);
        ProtectedLicenseSecrets? secrets = JsonSerializer.Deserialize<ProtectedLicenseSecrets>(rawBytes, Options);
        return secrets ?? new ProtectedLicenseSecrets();
    }

    public async Task SaveAsync(ProtectedLicenseSecrets secrets, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(secrets);

        Directory.CreateDirectory(_paths.DataRoot);
        byte[] rawBytes = JsonSerializer.SerializeToUtf8Bytes(secrets, Options);
        byte[] protectedBytes = WindowsDataProtection.Protect(rawBytes);
        await File.WriteAllBytesAsync(_paths.ProtectedLicenseSecretsPath, protectedBytes, cancellationToken).ConfigureAwait(false);
    }

    public Task DeleteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (File.Exists(_paths.ProtectedLicenseSecretsPath))
        {
            File.Delete(_paths.ProtectedLicenseSecretsPath);
        }

        return Task.CompletedTask;
    }
}

public sealed class LauncherLicenseService
{
    private static readonly TimeSpan TrustedTimeRegressionTolerance = TimeSpan.FromMinutes(2);

    private readonly LauncherStateStore _stateStore;
    private readonly ProtectedLicenseSecretStore _secretStore;
    private readonly LauncherLicenseClient _licenseClient;
    private readonly LicenseTrustSettings _trustSettings;

    public LauncherLicenseService(LauncherStateStore stateStore, ProtectedLicenseSecretStore secretStore, LauncherLicenseClient licenseClient, LicenseTrustSettings? trustSettings = null)
    {
        _stateStore = stateStore;
        _secretStore = secretStore;
        _licenseClient = licenseClient;
        _trustSettings = trustSettings ?? new LicenseTrustSettings();
    }

    public LicenseGateState CurrentState { get; private set; } = new()
    {
        AccessMode = LicenseAccessMode.Unlicensed,
        StatusMessage = "License has not been validated yet."
    };

    public async Task<LicenseGateState> InitializeAsync(LauncherState state, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);
        ProtectedLicenseSecrets? secrets = await TryLoadSecretsAsync().ConfigureAwait(false);
        string recoveredKey = secrets?.LicenseKey ?? string.Empty;
        Uri serviceBaseUri = _trustSettings.ResolveServiceBaseUri(state.DeveloperLicenseServerUrlOverride);
        PruneReplayCache(state, DateTimeOffset.UtcNow);

        if (string.IsNullOrWhiteSpace(recoveredKey))
        {
            state.LicenseSummary = null;
            await _stateStore.SaveAsync(state, cancellationToken).ConfigureAwait(false);
            return SetCurrentState(new LicenseGateState
            {
                AccessMode = LicenseAccessMode.Unlicensed,
                EffectiveServerUrl = serviceBaseUri.AbsoluteUri,
                StatusMessage = "Enter a license key before validating.",
                RecoveredLicenseKey = string.Empty
            });
        }

        string machineBinding = HardwareFingerprintProvider.CreateSignal();
        try
        {
            LicenseVerificationResult result = await _licenseClient.StartSessionAsync(
                serviceBaseUri,
                recoveredKey,
                machineBinding,
                Environment.MachineName,
                state.ReplayCache,
                secrets?.TrustedTimeSnapshot,
                cancellationToken).ConfigureAwait(false);

            OfflineLicenseGrant? offlineGrant = await TryIssueOfflineGrantAsync(serviceBaseUri, result, state, cancellationToken).ConfigureAwait(false);
            await PersistValidatedSessionAsync(state, recoveredKey, machineBinding, result, offlineGrant, cancellationToken).ConfigureAwait(false);
            return SetCurrentState(CreateLicensedState(result.Summary, recoveredKey, serviceBaseUri.AbsoluteUri, isOffline: false));
        }
        catch (Exception ex) when (IsConnectivityFailure(ex))
        {
            if (TryBuildOfflineState(state, secrets, machineBinding, serviceBaseUri, out LicenseGateState? offlineState))
            {
                await _stateStore.SaveAsync(state, cancellationToken).ConfigureAwait(false);
                return SetCurrentState(offlineState!);
            }

            return SetCurrentState(new LicenseGateState
            {
                AccessMode = LicenseAccessMode.Unlicensed,
                EffectiveServerUrl = serviceBaseUri.AbsoluteUri,
                StatusMessage = $"Unable to reach the license server: {ex.Message}",
                RecoveredLicenseKey = recoveredKey
            });
        }
        catch (LicenseValidationException ex)
        {
            await PersistRecoveredKeyOnlyAsync(recoveredKey, cancellationToken).ConfigureAwait(false);
            state.LicenseSummary = null;
            await _stateStore.SaveAsync(state, cancellationToken).ConfigureAwait(false);
            return SetCurrentState(new LicenseGateState
            {
                AccessMode = LicenseAccessMode.Unlicensed,
                EffectiveServerUrl = serviceBaseUri.AbsoluteUri,
                StatusMessage = $"License validation failed: {ex.Message}",
                RecoveredLicenseKey = recoveredKey
            });
        }
    }

    public async Task<LicenseGateState> ValidateAsync(LauncherState state, string licenseKey, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);

        string normalizedKey = licenseKey.Trim();
        Uri serviceBaseUri = _trustSettings.ResolveServiceBaseUri(state.DeveloperLicenseServerUrlOverride);
        string machineBinding = HardwareFingerprintProvider.CreateSignal();

        LicenseVerificationResult result = await _licenseClient.StartSessionAsync(
            serviceBaseUri,
            normalizedKey,
            machineBinding,
            Environment.MachineName,
            state.ReplayCache,
            (await TryLoadSecretsAsync().ConfigureAwait(false))?.TrustedTimeSnapshot,
            cancellationToken).ConfigureAwait(false);

        OfflineLicenseGrant? offlineGrant = await TryIssueOfflineGrantAsync(serviceBaseUri, result, state, cancellationToken).ConfigureAwait(false);
        await PersistValidatedSessionAsync(state, normalizedKey, machineBinding, result, offlineGrant, cancellationToken).ConfigureAwait(false);
        return SetCurrentState(CreateLicensedState(result.Summary, normalizedKey, serviceBaseUri.AbsoluteUri, isOffline: false));
    }

    public async Task<LicenseGateState> HeartbeatAsync(LauncherState state, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);

        ProtectedLicenseSecrets? secrets = await TryLoadSecretsAsync().ConfigureAwait(false);
        if (secrets is null || string.IsNullOrWhiteSpace(secrets.LicenseKey) || string.IsNullOrWhiteSpace(secrets.SessionToken))
        {
            return SetCurrentState(new LicenseGateState
            {
                AccessMode = LicenseAccessMode.Unlicensed,
                StatusMessage = "The launcher does not have an active licensed session.",
                RecoveredLicenseKey = secrets?.LicenseKey ?? string.Empty
            });
        }

        Uri serviceBaseUri = _trustSettings.ResolveServiceBaseUri(state.DeveloperLicenseServerUrlOverride);
        string machineBinding = HardwareFingerprintProvider.CreateSignal();
        try
        {
            LicenseVerificationResult result = await _licenseClient.HeartbeatAsync(
                serviceBaseUri,
                secrets.LicenseKey,
                secrets.SessionToken,
                machineBinding,
                state.ReplayCache,
                secrets.TrustedTimeSnapshot,
                cancellationToken).ConfigureAwait(false);

            OfflineLicenseGrant? offlineGrant = await TryIssueOfflineGrantAsync(serviceBaseUri, result, state, cancellationToken).ConfigureAwait(false);
            await PersistValidatedSessionAsync(state, secrets.LicenseKey, machineBinding, result, offlineGrant ?? secrets.OfflineGrant, cancellationToken).ConfigureAwait(false);
            return SetCurrentState(CreateLicensedState(result.Summary, secrets.LicenseKey, serviceBaseUri.AbsoluteUri, isOffline: false));
        }
        catch (Exception ex) when (IsConnectivityFailure(ex))
        {
            if (TryBuildOfflineState(state, secrets, machineBinding, serviceBaseUri, out LicenseGateState? offlineState))
            {
                await _stateStore.SaveAsync(state, cancellationToken).ConfigureAwait(false);
                return SetCurrentState(offlineState!);
            }

            return SetCurrentState(new LicenseGateState
            {
                AccessMode = LicenseAccessMode.Unlicensed,
                EffectiveServerUrl = serviceBaseUri.AbsoluteUri,
                StatusMessage = $"Heartbeat failed and no valid offline grant exists: {ex.Message}",
                RecoveredLicenseKey = secrets.LicenseKey
            });
        }
        catch (LicenseValidationException ex)
        {
            await PersistRecoveredKeyOnlyAsync(secrets.LicenseKey, cancellationToken).ConfigureAwait(false);
            state.LicenseSummary = null;
            await _stateStore.SaveAsync(state, cancellationToken).ConfigureAwait(false);
            return SetCurrentState(new LicenseGateState
            {
                AccessMode = LicenseAccessMode.Unlicensed,
                EffectiveServerUrl = serviceBaseUri.AbsoluteUri,
                StatusMessage = $"Heartbeat rejected: {ex.Message}",
                RecoveredLicenseKey = secrets.LicenseKey
            });
        }
    }

    private static void PruneReplayCache(LauncherState state, DateTimeOffset nowUtc)
    {
        state.ReplayCache.RemoveAll(entry => entry.ExpiresAtUtc <= nowUtc);
    }

    private static LicenseGateState CreateLicensedState(LauncherLicenseSummary summary, string recoveredKey, string serverUrl, bool isOffline)
    {
        return new LicenseGateState
        {
            AccessMode = isOffline ? LicenseAccessMode.Offline : LicenseAccessMode.Licensed,
            EffectiveServerUrl = serverUrl,
            RecoveredLicenseKey = recoveredKey,
            Summary = summary,
            StatusMessage = isOffline
                ? $"Offline license grant active until {summary.ExpiresAtUtc:u}."
                : $"License active until {summary.ExpiresAtUtc:u}.",
        };
    }

    private async Task PersistValidatedSessionAsync(
        LauncherState state,
        string licenseKey,
        string machineBinding,
        LicenseVerificationResult result,
        OfflineLicenseGrant? offlineGrant,
        CancellationToken cancellationToken)
    {
        state.LicenseSummary = result.Summary;
        PruneReplayCache(state, result.ServerTimeUtc);
        state.ReplayCache.RemoveAll(entry => string.Equals(entry.ResponseId, result.ResponseId, StringComparison.Ordinal));
        state.ReplayCache.Add(new ReplayCacheEntry
        {
            ResponseId = result.ResponseId,
            ExpiresAtUtc = result.Summary.ExpiresAtUtc
        });

        await _secretStore.SaveAsync(new ProtectedLicenseSecrets
        {
            LicenseKey = licenseKey,
            SessionToken = result.SessionToken,
            MachineBinding = machineBinding,
            TrustedTimeSnapshot = result.TrustedTimeSnapshot,
            OfflineGrant = offlineGrant
        }, cancellationToken).ConfigureAwait(false);

        await _stateStore.SaveAsync(state, cancellationToken).ConfigureAwait(false);
    }

    private async Task PersistRecoveredKeyOnlyAsync(string licenseKey, CancellationToken cancellationToken)
    {
        await _secretStore.SaveAsync(new ProtectedLicenseSecrets
        {
            LicenseKey = licenseKey
        }, cancellationToken).ConfigureAwait(false);
    }

    private async Task<ProtectedLicenseSecrets?> TryLoadSecretsAsync()
    {
        try
        {
            return await _secretStore.LoadAsync().ConfigureAwait(false);
        }
        catch
        {
            await _secretStore.DeleteAsync().ConfigureAwait(false);
            return null;
        }
    }

    private async Task<OfflineLicenseGrant?> TryIssueOfflineGrantAsync(Uri serviceBaseUri, LicenseVerificationResult result, LauncherState state, CancellationToken cancellationToken)
    {
        if (!result.Summary.Policy.OfflineTokenAllowed || string.IsNullOrWhiteSpace(result.SessionToken))
        {
            return null;
        }

        try
        {
            OfflineLicenseGrant offlineGrant = await _licenseClient.IssueOfflineGrantAsync(
                serviceBaseUri,
                result.SessionToken,
                result.MachineBinding,
                state.ReplayCache,
                result.TrustedTimeSnapshot,
                cancellationToken).ConfigureAwait(false);

            state.ReplayCache.RemoveAll(entry => string.Equals(entry.ResponseId, offlineGrant.ResponseId, StringComparison.Ordinal));
            state.ReplayCache.Add(new ReplayCacheEntry
            {
                ResponseId = offlineGrant.ResponseId,
                ExpiresAtUtc = offlineGrant.ExpiresAtUtc
            });

            return offlineGrant;
        }
        catch
        {
            return null;
        }
    }

    private bool TryBuildOfflineState(
        LauncherState state,
        ProtectedLicenseSecrets? secrets,
        string machineBinding,
        Uri serviceBaseUri,
        out LicenseGateState? gateState)
    {
        gateState = null;
        if (secrets?.OfflineGrant is null || secrets.TrustedTimeSnapshot is null)
        {
            return false;
        }

        DateTimeOffset trustedNow = DeriveTrustedNow(secrets.TrustedTimeSnapshot);
        OfflineLicenseGrant grant = secrets.OfflineGrant;
        if (!string.Equals(grant.MachineBinding, machineBinding, StringComparison.Ordinal) ||
            !string.Equals(grant.ProductCode, LauncherDefaults.ProductCode, StringComparison.OrdinalIgnoreCase) ||
            trustedNow < grant.NotBeforeUtc ||
            trustedNow > grant.ExpiresAtUtc)
        {
            return false;
        }

        state.LicenseSummary = new LauncherLicenseSummary
        {
            LicenseId = grant.LicenseId,
            ProductCode = grant.ProductCode,
            Status = grant.Status,
            ExpiresAtUtc = grant.ExpiresAtUtc,
            LastValidatedAtUtc = grant.ServerTimeUtc,
            IsOfflineMode = true,
            Policy = ClonePolicy(grant.Policy)
        };

        gateState = CreateLicensedState(state.LicenseSummary, secrets.LicenseKey, serviceBaseUri.AbsoluteUri, isOffline: true);
        return true;
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

    private static DateTimeOffset DeriveTrustedNow(TrustedTimeSnapshot snapshot)
    {
        long nowTicks = Environment.TickCount64;
        long delta = nowTicks - snapshot.TickCount64;
        if (delta < 0)
        {
            delta = 0;
        }

        return snapshot.TrustedUtc.AddMilliseconds(delta);
    }

    internal static void ValidateTrustedTimeWindow(TrustedTimeSnapshot? priorSnapshot, DateTimeOffset serverTimeUtc)
    {
        if (priorSnapshot is null)
        {
            return;
        }

        DateTimeOffset derivedTrustedNow = DeriveTrustedNow(priorSnapshot);
        if (serverTimeUtc + TrustedTimeRegressionTolerance < derivedTrustedNow)
        {
            throw new LicenseValidationException("Trusted server time regressed beyond the allowed tolerance.", null);
        }
    }

    private static bool IsConnectivityFailure(Exception ex)
    {
        return ex is HttpRequestException || ex is TaskCanceledException;
    }

    private LicenseGateState SetCurrentState(LicenseGateState state)
    {
        CurrentState = state;
        return state;
    }
}

internal sealed class SignedLicenseRequest
{
    public string LicenseKey { get; set; } = string.Empty;

    public string ProductCode { get; set; } = string.Empty;

    public string HWIDSignal { get; set; } = string.Empty;

    public string MachineTag { get; set; } = string.Empty;

    public string UserAgent { get; set; } = string.Empty;

    public string ClientNonce { get; set; } = string.Empty;
}

internal sealed class HeartbeatRequest
{
    public string SessionToken { get; set; } = string.Empty;

    public string ClientNonce { get; set; } = string.Empty;
}

internal sealed class OfflineGrantRequest
{
    public string SessionToken { get; set; } = string.Empty;

    public string ClientNonce { get; set; } = string.Empty;
}

internal sealed class SignedResponseEnvelopeDto
{
    public JsonElement Payload { get; set; }

    public string SignatureBase64 { get; set; } = string.Empty;

    public string Algorithm { get; set; } = string.Empty;

    public string KeyId { get; set; } = string.Empty;
}

internal sealed class SignedSessionPayload
{
    public string ResponseId { get; set; } = string.Empty;

    public string LicenseId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string ProductCode { get; set; } = string.Empty;

    public string MachineBinding { get; set; } = string.Empty;

    public DateTimeOffset IssuedAtUtc { get; set; }

    public DateTimeOffset NotBeforeUtc { get; set; }

    public DateTimeOffset ExpiresAtUtc { get; set; }

    public DateTimeOffset ServerTimeUtc { get; set; }

    public string ServerNonce { get; set; } = string.Empty;

    public string ClientNonce { get; set; } = string.Empty;

    public string SessionToken { get; set; } = string.Empty;

    public string SessionTokenId { get; set; } = string.Empty;

    public LicensePolicy Policy { get; set; } = new();
}

internal sealed class SignedOfflineGrantPayload
{
    public string ResponseId { get; set; } = string.Empty;

    public string LicenseId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string ProductCode { get; set; } = string.Empty;

    public string MachineBinding { get; set; } = string.Empty;

    public DateTimeOffset IssuedAtUtc { get; set; }

    public DateTimeOffset NotBeforeUtc { get; set; }

    public DateTimeOffset ExpiresAtUtc { get; set; }

    public DateTimeOffset ServerTimeUtc { get; set; }

    public string ServerNonce { get; set; } = string.Empty;

    public string ClientNonce { get; set; } = string.Empty;

    public string OfflineToken { get; set; } = string.Empty;

    public string SessionTokenId { get; set; } = string.Empty;

    public LicensePolicy Policy { get; set; } = new();
}

internal static class LauncherLicenseJson
{
    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static byte[] GetPayloadBytes(string payloadJson)
    {
        return Encoding.UTF8.GetBytes(payloadJson);
    }
}
