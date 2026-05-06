using System.IO.Compression;
using System.Text.Json;

namespace UOAIO.Update;

public sealed class UpdateCoordinator
{
    private readonly BootstrapperSettings _settings;
    private readonly ReleaseManifestVerifier _verifier;
    private readonly HttpClient _httpClient;

    public UpdateCoordinator(BootstrapperSettings settings, ReleaseManifestVerifier verifier, HttpClient? httpClient = null)
    {
        _settings = settings;
        _verifier = verifier;
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<UpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        EnsureLayout();
        SignedReleaseManifest envelope = await _verifier
            .LoadAndVerifyAsync(new Uri(_settings.ManifestUri, UriKind.Absolute), _settings.PublicKeyPemPath, cancellationToken)
            .ConfigureAwait(false);

        InstallState state = await LoadInstallStateAsync(cancellationToken).ConfigureAwait(false);
        UpdateCheckResult result = new();

        foreach (ReleasePackage package in envelope.Manifest.Packages)
        {
            state.InstalledPackages.TryGetValue(package.PackageId, out string? currentVersion);
            if (!string.Equals(currentVersion, package.Version, StringComparison.OrdinalIgnoreCase))
            {
                result.Packages.Add(new PackageUpdate
                {
                    CurrentVersion = currentVersion ?? string.Empty,
                    TargetPackage = package
                });
            }
        }

        return result;
    }

    public async Task<StagedUpdatePlan> StageUpdatesAsync(UpdateCheckResult result, CancellationToken cancellationToken = default)
    {
        EnsureLayout();
        InstallState state = await LoadInstallStateAsync(cancellationToken).ConfigureAwait(false);
        StagedUpdatePlan plan = new() { PriorState = state };

        foreach (PackageUpdate update in result.Packages)
        {
            string packageStageRoot = GetStagingPackagePath(update.TargetPackage.PackageId, update.TargetPackage.Version);
            ResetDirectory(packageStageRoot);
            string zipPath = Path.Combine(GetDownloadsRoot(), $"{update.TargetPackage.PackageId}-{update.TargetPackage.Version}.zip");
            await DownloadPackageAsync(update.TargetPackage, zipPath, cancellationToken).ConfigureAwait(false);

            string actualHash = ReleaseManifestSerializer.ComputeSha256(zipPath);
            if (!string.Equals(actualHash, update.TargetPackage.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Hash verification failed for package '{update.TargetPackage.PackageId}'.");
            }

            ZipFile.ExtractToDirectory(zipPath, packageStageRoot, overwriteFiles: true);
            plan.Packages.Add(new StagedPackage
            {
                PackageId = update.TargetPackage.PackageId,
                Version = update.TargetPackage.Version,
                StagingPath = packageStageRoot
            });
        }

        await SavePriorStateAsync(state, cancellationToken).ConfigureAwait(false);
        return plan;
    }

    public async Task ApplyUpdatesAsync(StagedUpdatePlan plan, CancellationToken cancellationToken = default)
    {
        EnsureLayout();
        InstallState installState = await LoadInstallStateAsync(cancellationToken).ConfigureAwait(false);

        foreach (StagedPackage package in plan.Packages)
        {
            string currentPath = GetCurrentPackagePath(package.PackageId);
            string previousPath = GetPreviousPackagePath(package.PackageId);
            ValidateUnderInstallRoot(currentPath);
            ValidateUnderInstallRoot(previousPath);

            if (Directory.Exists(previousPath))
            {
                Directory.Delete(previousPath, recursive: true);
            }

            if (Directory.Exists(currentPath))
            {
                Directory.Move(currentPath, previousPath);
            }

            Directory.Move(package.StagingPath, currentPath);
            installState.InstalledPackages[package.PackageId] = package.Version;
        }

        installState.LastSuccessfulApplyUtc = DateTimeOffset.UtcNow;
        await SaveInstallStateAsync(installState, cancellationToken).ConfigureAwait(false);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        EnsureLayout();
        InstallState priorState = await LoadPriorStateAsync(cancellationToken).ConfigureAwait(false);
        foreach ((string packageId, _) in priorState.InstalledPackages)
        {
            string currentPath = GetCurrentPackagePath(packageId);
            string previousPath = GetPreviousPackagePath(packageId);
            ValidateUnderInstallRoot(currentPath);
            ValidateUnderInstallRoot(previousPath);

            if (!Directory.Exists(previousPath))
            {
                continue;
            }

            if (Directory.Exists(currentPath))
            {
                Directory.Delete(currentPath, recursive: true);
            }

            Directory.Move(previousPath, currentPath);
        }

        priorState.LastSuccessfulApplyUtc = DateTimeOffset.UtcNow;
        await SaveInstallStateAsync(priorState, cancellationToken).ConfigureAwait(false);
    }

    private async Task DownloadPackageAsync(ReleasePackage package, string zipPath, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(zipPath)!);
        Uri packageUri = new Uri(new Uri(_settings.ManifestUri, UriKind.Absolute), package.DownloadUrl);
        if (packageUri.IsFile)
        {
            File.Copy(packageUri.LocalPath, zipPath, overwrite: true);
            return;
        }

        using Stream source = await _httpClient.GetStreamAsync(packageUri, cancellationToken).ConfigureAwait(false);
        using FileStream target = File.Create(zipPath);
        await source.CopyToAsync(target, cancellationToken).ConfigureAwait(false);
    }

    private void EnsureLayout()
    {
        Directory.CreateDirectory(_settings.InstallRoot);
        Directory.CreateDirectory(GetCurrentRoot());
        Directory.CreateDirectory(GetPreviousRoot());
        Directory.CreateDirectory(GetStagingRoot());
        Directory.CreateDirectory(GetDownloadsRoot());
    }

    private string GetCurrentRoot() => Path.Combine(_settings.InstallRoot, "current");

    private string GetPreviousRoot() => Path.Combine(_settings.InstallRoot, "previous");

    private string GetStagingRoot() => Path.Combine(_settings.InstallRoot, "staging");

    private string GetDownloadsRoot() => Path.Combine(_settings.InstallRoot, "downloads");

    private string GetInstallStatePath() => Path.Combine(_settings.InstallRoot, "install-state.json");

    private string GetPriorStatePath() => Path.Combine(_settings.InstallRoot, "prior-install-state.json");

    private string GetCurrentPackagePath(string packageId) => Path.Combine(GetCurrentRoot(), packageId);

    private string GetPreviousPackagePath(string packageId) => Path.Combine(GetPreviousRoot(), packageId);

    private string GetStagingPackagePath(string packageId, string version) => Path.Combine(GetStagingRoot(), packageId, version);

    private void ResetDirectory(string path)
    {
        ValidateUnderInstallRoot(path);
        if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }

        Directory.CreateDirectory(path);
    }

    private void ValidateUnderInstallRoot(string path)
    {
        string root = Path.GetFullPath(_settings.InstallRoot);
        string target = Path.GetFullPath(path);
        if (!target.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Resolved path is outside the install root.");
        }
    }

    private async Task<InstallState> LoadInstallStateAsync(CancellationToken cancellationToken)
    {
        string path = GetInstallStatePath();
        if (!File.Exists(path))
        {
            return new InstallState();
        }

        string json = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<InstallState>(json, ReleaseManifestSerializer.SerializerOptions) ?? new InstallState();
    }

    private Task SaveInstallStateAsync(InstallState state, CancellationToken cancellationToken)
    {
        string json = JsonSerializer.Serialize(state, ReleaseManifestSerializer.SerializerOptions);
        return File.WriteAllTextAsync(GetInstallStatePath(), json, cancellationToken);
    }

    private Task SavePriorStateAsync(InstallState state, CancellationToken cancellationToken)
    {
        string json = JsonSerializer.Serialize(state, ReleaseManifestSerializer.SerializerOptions);
        return File.WriteAllTextAsync(GetPriorStatePath(), json, cancellationToken);
    }

    private async Task<InstallState> LoadPriorStateAsync(CancellationToken cancellationToken)
    {
        string path = GetPriorStatePath();
        if (!File.Exists(path))
        {
            return new InstallState();
        }

        string json = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<InstallState>(json, ReleaseManifestSerializer.SerializerOptions) ?? new InstallState();
    }
}
