using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace UOAIO.Update;

public sealed class BootstrapperSettings
{
    public string Channel { get; set; } = "stable";

    public string ManifestUri { get; set; } = string.Empty;

    public string InstallRoot { get; set; } = string.Empty;

    public string PublicKeyPemPath { get; set; } = string.Empty;
}

public sealed class ReleaseManifest
{
    public string SchemaVersion { get; set; } = "1.0";

    public string Channel { get; set; } = "stable";

    public DateTimeOffset PublishedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public List<ReleasePackage> Packages { get; set; } = new();
}

public sealed class ReleasePackage
{
    public string PackageId { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string DownloadUrl { get; set; } = string.Empty;

    public string Sha256 { get; set; } = string.Empty;

    public string EntryPoint { get; set; } = string.Empty;
}

public sealed class SignedReleaseManifest
{
    public string SignatureBase64 { get; set; } = string.Empty;

    public ReleaseManifest Manifest { get; set; } = new();
}

public sealed class InstallState
{
    public Dictionary<string, string> InstalledPackages { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public DateTimeOffset LastSuccessfulApplyUtc { get; set; }
}

public sealed class UpdateCheckResult
{
    public bool HasUpdates => Packages.Count > 0;

    public List<PackageUpdate> Packages { get; set; } = new();
}

public sealed class PackageUpdate
{
    public string CurrentVersion { get; set; } = string.Empty;

    public ReleasePackage TargetPackage { get; set; } = new();
}

public sealed class StagedUpdatePlan
{
    public List<StagedPackage> Packages { get; set; } = new();

    public InstallState PriorState { get; set; } = new();
}

public sealed class StagedPackage
{
    public string PackageId { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string StagingPath { get; set; } = string.Empty;
}

public static class ReleaseManifestSerializer
{
    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static string Canonicalize(ReleaseManifest manifest)
    {
        return JsonSerializer.Serialize(manifest, SerializerOptions);
    }

    public static string ComputeSha256(string filePath)
    {
        using FileStream stream = File.OpenRead(filePath);
        using SHA256 sha256 = SHA256.Create();
        byte[] hash = sha256.ComputeHash(stream);
        return Convert.ToHexString(hash);
    }

    public static byte[] GetManifestBytes(ReleaseManifest manifest)
    {
        return Encoding.UTF8.GetBytes(Canonicalize(manifest));
    }
}
