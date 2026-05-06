using System.Text.Json;
using UOAIO.Update;

namespace UOAIO.Bootstrapper;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        try
        {
            string settingsPath = args.Length > 0
                ? Path.GetFullPath(args[0])
                : Path.Combine(AppContext.BaseDirectory, "bootstrapper.settings.json");

            if (!File.Exists(settingsPath))
            {
                Console.Error.WriteLine($"Bootstrapper settings file not found: {settingsPath}");
                return 1;
            }

            BootstrapperSettings settings = JsonSerializer.Deserialize<BootstrapperSettings>(
                    await File.ReadAllTextAsync(settingsPath).ConfigureAwait(false),
                    ReleaseManifestSerializer.SerializerOptions)
                ?? throw new InvalidOperationException("Bootstrapper settings could not be parsed.");

            UpdateCoordinator coordinator = new(settings, new ReleaseManifestVerifier());
            UpdateCheckResult check = await coordinator.CheckForUpdatesAsync().ConfigureAwait(false);
            if (!check.HasUpdates)
            {
                Console.WriteLine("No updates available.");
                return 0;
            }

            Console.WriteLine($"Found {check.Packages.Count} update(s).");
            foreach (PackageUpdate package in check.Packages)
            {
                Console.WriteLine($" - {package.TargetPackage.PackageId}: {package.CurrentVersion} -> {package.TargetPackage.Version}");
            }

            StagedUpdatePlan plan = await coordinator.StageUpdatesAsync(check).ConfigureAwait(false);
            await coordinator.ApplyUpdatesAsync(plan).ConfigureAwait(false);
            Console.WriteLine("Updates applied successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Bootstrapper failed: {ex.Message}");
            return 1;
        }
    }
}
