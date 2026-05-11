using System;
using System.Collections.Generic;
using UOAIO;
using UOAIO.ShardRuntime;

namespace UOAIO.Client.Host;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        int exitCode = 1;
        try
        {
            Dictionary<string, string> parsedArgs = ParseArgs(args);
            if (!parsedArgs.TryGetValue("bootstrap-pipe", out string pipeName) || string.IsNullOrWhiteSpace(pipeName))
            {
                Console.Error.WriteLine("A bootstrap pipe name is required. Use --bootstrap-pipe <name>.");
                exitCode = 1;
            }
            else if (!parsedArgs.TryGetValue("runtime-data-root", out string runtimeDataRoot) || string.IsNullOrWhiteSpace(runtimeDataRoot))
            {
                Console.Error.WriteLine("A runtime data root is required. Use --runtime-data-root <path>.");
                exitCode = 1;
            }
            else
            {
                ClientBootstrapPipeTransport transport = new();
                ClientBootstrapDefinition bootstrap = transport.Read(pipeName);
                Engine.Run(bootstrap, runtimeDataRoot);
                exitCode = 0;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Ultima.Client.Host failed: " + ex.Message);
            exitCode = 1;
        }
        finally
        {
            Environment.Exit(exitCode);
        }

        return exitCode;
    }

    private static Dictionary<string, string> ParseArgs(string[] args)
    {
        Dictionary<string, string> parsedArgs = new(StringComparer.OrdinalIgnoreCase);
        if (args == null || args.Length == 0)
        {
            return parsedArgs;
        }

        for (int i = 0; i < args.Length; i++)
        {
            string current = args[i];
            if (string.IsNullOrWhiteSpace(current))
            {
                continue;
            }

            string key = current.Trim();
            if (key.StartsWith("--", StringComparison.Ordinal))
            {
                key = key.Substring(2);
            }
            else if (key.StartsWith("-", StringComparison.Ordinal) || key.StartsWith("/", StringComparison.Ordinal))
            {
                key = key.Substring(1);
            }

            int separatorIndex = key.IndexOf('=');
            if (separatorIndex < 0)
            {
                separatorIndex = key.IndexOf(':');
            }

            if (separatorIndex > 0)
            {
                parsedArgs[key.Substring(0, separatorIndex).Trim()] = key.Substring(separatorIndex + 1).Trim();
                continue;
            }

            if (i + 1 < args.Length)
            {
                string value = args[i + 1];
                if (!string.IsNullOrWhiteSpace(value) && !value.StartsWith("-", StringComparison.Ordinal) && !value.StartsWith("/", StringComparison.Ordinal))
                {
                    parsedArgs[key] = value;
                    i++;
                    continue;
                }
            }

            parsedArgs[key] = string.Empty;
        }

        return parsedArgs;
    }
}
