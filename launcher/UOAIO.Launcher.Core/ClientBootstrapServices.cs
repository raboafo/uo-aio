using System.Diagnostics;
using UOAIO.ShardRuntime;

namespace UOAIO.Launcher.Core;

public static class ClientBootstrapDefinitionFactory
{
    public static ClientBootstrapDefinition Create(ShardDefinition shard)
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

        return new ClientBootstrapDefinition
        {
            SchemaVersion = 2,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            Shard = CloneShard(shard)
        };
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
            ClientVersion = new Version(shard.GetVersionString()),
            ServerIP = shard.ServerIP,
            ServerPort = shard.ServerPort,
            Metadata = new Dictionary<string, string>(shard.Metadata ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase)
        };
    }
}

public sealed class ClientProcessLauncher
{
    private static readonly string[] PreferredExecutableNames =
    {
        "Ultima.Client.Host.exe"
    };

    public ProcessStartInfo CreateStartInfo(string appBaseDirectory, string bootstrapPipeName)
    {
        if (string.IsNullOrWhiteSpace(bootstrapPipeName))
        {
            throw new ArgumentException("A bootstrap pipe name is required.", nameof(bootstrapPipeName));
        }

        string executablePath = ResolveClientExecutablePath(appBaseDirectory);
        string runtimeDataDirectory = ResolveRuntimeDataRoot();
        return new ProcessStartInfo(executablePath)
        {
            Arguments = $"--bootstrap-pipe \"{bootstrapPipeName}\" --runtime-data-root \"{runtimeDataDirectory}\"",
            UseShellExecute = false,
            WorkingDirectory = Path.GetDirectoryName(executablePath) ?? appBaseDirectory
        };
    }

    public Process Start(string appBaseDirectory, string bootstrapPipeName)
    {
        ProcessStartInfo startInfo = CreateStartInfo(appBaseDirectory, bootstrapPipeName);
        return Process.Start(startInfo) ?? throw new InvalidOperationException("Client process did not start.");
    }

    public async Task<Process> StartWithBootstrapAsync(string appBaseDirectory, ClientBootstrapDefinition bootstrap, CancellationToken cancellationToken = default)
    {
        string pipeName = ClientBootstrapPipeTransport.CreatePipeName();
        using CancellationTokenSource pipeCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        ClientBootstrapPipeTransport transport = new();
        Task writeTask = transport.WriteAsync(pipeName, bootstrap, pipeCts.Token);

        try
        {
            Process process = Start(appBaseDirectory, pipeName);
            await writeTask.ConfigureAwait(false);
            return process;
        }
        catch
        {
            pipeCts.Cancel();
            try
            {
                await writeTask.ConfigureAwait(false);
            }
            catch
            {
            }

            throw;
        }
    }

    public string ResolveClientExecutablePath(string appBaseDirectory)
    {
        if (string.IsNullOrWhiteSpace(appBaseDirectory))
        {
            throw new ArgumentException("An application base directory is required.", nameof(appBaseDirectory));
        }

        HashSet<string> candidates = new(StringComparer.OrdinalIgnoreCase);
        foreach (string candidate in EnumerateCandidatePaths(appBaseDirectory))
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            string fullPath = Path.GetFullPath(candidate);
            if (!candidates.Add(fullPath))
            {
                continue;
            }

            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        throw new FileNotFoundException("Unable to locate Ultima.Client.Host.exe from the launcher directory.");
    }

    public string ResolveRuntimeDataRoot()
    {
        string runtimeDataRoot = GetRuntimeDataRootPath();
        Directory.CreateDirectory(runtimeDataRoot);
        return runtimeDataRoot;
    }

    private static string GetRuntimeDataRootPath()
    {
        string? overrideRoot = Environment.GetEnvironmentVariable("UOAIO_CLIENT_RUNTIME_ROOT");
        if (!string.IsNullOrWhiteSpace(overrideRoot))
        {
            return Path.GetFullPath(overrideRoot);
        }

        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(localAppData))
        {
            throw new InvalidOperationException("Unable to determine the LocalApplicationData folder for client runtime data.");
        }

        return Path.Combine(localAppData, "UOAIO");
    }

    private static IEnumerable<string> EnumerateCandidatePaths(string appBaseDirectory)
    {
        foreach (string executableName in PreferredExecutableNames)
        {
            yield return Path.Combine(appBaseDirectory, executableName);
            yield return Path.Combine(appBaseDirectory, Path.GetFileNameWithoutExtension(executableName), executableName);
        }

        DirectoryInfo? current = new DirectoryInfo(appBaseDirectory);
        while (current is not null)
        {
            foreach (string executableName in PreferredExecutableNames)
            {
                yield return Path.Combine(current.FullName, executableName);
                yield return Path.Combine(current.FullName, Path.GetFileNameWithoutExtension(executableName), executableName);
            }

            foreach (string hostPath in EnumerateProjectOutputCandidates(current.FullName, "Ultima.Client.Host", "Ultima.Client.Host.exe"))
            {
                yield return hostPath;
            }

            current = current.Parent;
        }
    }

    private static IEnumerable<string> EnumerateProjectOutputCandidates(string rootPath, string projectDirectoryName, string executableName)
    {
        string projectBinDirectory = Path.Combine(rootPath, "client", projectDirectoryName, "bin");
        if (!Directory.Exists(projectBinDirectory))
        {
            yield break;
        }

        foreach (string file in Directory.EnumerateFiles(projectBinDirectory, executableName, SearchOption.AllDirectories)
            .OrderByDescending(path => File.GetLastWriteTimeUtc(path)))
        {
            yield return file;
        }
    }
}
