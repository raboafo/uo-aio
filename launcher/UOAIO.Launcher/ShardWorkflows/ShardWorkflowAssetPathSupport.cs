using System.IO;
using System.Windows.Forms;
using UOAIO.ShardRuntime;

namespace UOAIO.Launcher.ShardWorkflows;

internal static class ShardWorkflowAssetPathSupport
{
    public static string GetAssetPath(ShardDefinition shard)
    {
        if (shard.Metadata.TryGetValue(ShardMetadataKeys.ClientAssetPath, out string? assetPath) &&
            !string.IsNullOrWhiteSpace(assetPath))
        {
            return assetPath;
        }

        return string.Empty;
    }

    public static bool TryNormalizeAssetPath(string? value, out string normalizedPath, out string? error)
    {
        normalizedPath = string.Empty;
        error = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        try
        {
            normalizedPath = Path.GetFullPath(value.Trim());
        }
        catch (Exception ex)
        {
            error = $"Asset directory is invalid: {ex.Message}";
            return false;
        }

        if (!Directory.Exists(normalizedPath))
        {
            error = "Asset directory does not exist.";
            return false;
        }

        return true;
    }

    public static void ApplyAssetPathMetadata(IDictionary<string, string> metadata, string? assetPath)
    {
        if (string.IsNullOrWhiteSpace(assetPath))
        {
            metadata.Remove(ShardMetadataKeys.ClientAssetPath);
            return;
        }

        metadata[ShardMetadataKeys.ClientAssetPath] = assetPath.Trim();
    }

    public static string? BrowseForAssetPath(string? currentPath)
    {
        using FolderBrowserDialog dialog = new()
        {
            Description = "Select the directory that contains the shard client assets.",
            ShowNewFolderButton = false
        };
        if (!string.IsNullOrWhiteSpace(currentPath) && Directory.Exists(currentPath))
        {
            dialog.SelectedPath = currentPath;
        }

        return dialog.ShowDialog() == DialogResult.OK ? dialog.SelectedPath : null;
    }
}
