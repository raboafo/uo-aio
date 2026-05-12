using System.Diagnostics;
using System.IO.Pipes;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using UOAIO.Launcher.Core;
using UOAIO.ShardRuntime;
using UOAIO.Update;

namespace UOAIO.Tests;

internal static class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };

    public static async Task<int> Main()
    {
        List<(string Name, Func<Task> Test)> tests = new()
        {
            ("Shard catalog parsing and shard-id workflow resolution", TestShardCatalogAndWorkflowRegistryAsync),
            ("Per-shard remembered state encryption and filtering", TestShardDefinitionStateStoreAsync),
            ("Client bootstrap payload creation and round-trip", TestClientBootstrapRoundTripAsync),
            ("UO New Dawn Discord OAuth authorization service", TestUoNewDawnAuthorizationServiceAsync),
            ("UO New Dawn workflow state machine", TestUoNewDawnWorkflowStateMachineAsync),
            ("Active shard state round-trip", TestActiveShardStateRoundTripAsync),
            ("License client success and failure handling", TestLicenseClientAsync),
            ("Release manifest signature verification", TestManifestVerificationAsync),
            ("Bootstrapper stage/apply/rollback", TestUpdateCoordinatorAsync)
        };

        int failures = 0;
        foreach ((string name, Func<Task> test) in tests)
        {
            try
            {
                await test().ConfigureAwait(false);
                Console.WriteLine($"PASS  {name}");
            }
            catch (Exception ex)
            {
                failures++;
                Console.WriteLine($"FAIL  {name}");
                Console.WriteLine($"      {ex.Message}");
            }
        }

        Console.WriteLine(failures == 0
            ? $"All {tests.Count} tests passed."
            : $"{failures} of {tests.Count} tests failed.");

        return failures == 0 ? 0 : 1;
    }

    private static async Task TestShardCatalogAndWorkflowRegistryAsync()
    {
        string root = CreateTempDirectory();
        try
        {
            string manifestPath = Path.Combine(root, "shards.json");
            await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(new ShardCatalog
            {
                Shards = new List<ShardDefinition>
                {
                    new()
                    {
                        Id = "new-renaissance",
                        Name = "New Renaissance",
                        Description = "Test shard",
                        Host = "play.newrenaissanceuo.com",
                        ClientVersion = new Version(0, 0, 0, 0),
                        ServerPort = 2593,
                        Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        {
                            ["displayGroup"] = "recommended"
                        }
                    },
                    new()
                    {
                        Id = "tides-of-power",
                        Name = "Tides of Power",
                        Description = "Test shard",
                        Host = "login.uotides.com",
                        ClientVersion = new Version(0, 0, 0, 0),
                        ServerPort = 2593,
                        Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        {
                            ["displayGroup"] = "recommended"
                        }
                    }
                }
            }, JsonOptions)).ConfigureAwait(false);

            ShardCatalogService catalogService = new();
            ShardCatalog catalog = await catalogService.LoadAsync(manifestPath).ConfigureAwait(false);
            Assert(catalog.Shards.Count == 2, "Expected two shards.");
            Assert(!catalog.Shards[0].Metadata.ContainsKey("adapterId"), "Manifest should not depend on adapter ids.");
            Assert(catalog.Shards[0].ClientVersion?.ToString() == "0.0.0.0", "Expected shard client version to round-trip from the manifest.");

            ShardWorkflowRegistry<string> workflows = new(new[]
            {
                new ShardWorkflowRegistration<string>("new-renaissance", "new-ren-flow"),
                new ShardWorkflowRegistration<string>("uo-new-dawn", "uond-flow"),
                new ShardWorkflowRegistration<string>("tides-of-power", "top-flow")
            });

            string workflowId = workflows.Resolve(catalog.Shards[0].Id);
            Assert(workflowId == "new-ren-flow", "Expected shard workflow registry to resolve by shard id.");
            Assert(workflows.Resolve("tides-of-power") == "top-flow", "Expected Tides of Power workflow registry entry.");

            ShardDefinition runtimeShard = new()
            {
                Id = catalog.Shards[0].Id,
                Name = catalog.Shards[0].Name,
                Description = catalog.Shards[0].Description,
                Host = catalog.Shards[0].Host,
                Account = "tester",
                Password = "secret",
                ClientVersion = catalog.Shards[0].ClientVersion,
                ServerIP = IPAddress.Parse("203.0.113.10"),
                ServerPort = 2593,
                Metadata = new Dictionary<string, string>(catalog.Shards[0].Metadata, StringComparer.OrdinalIgnoreCase)
            };

            ClientBootstrapDefinition bootstrap = ClientBootstrapDefinitionFactory.Create(runtimeShard);

            Assert(bootstrap.SchemaVersion == 2, "Expected client bootstrap schema version 2.");
            Assert(bootstrap.Shard.ServerIP!.ToString() == "203.0.113.10", "Expected resolved IP address to be persisted.");
            Assert(bootstrap.Shard.Account == "tester", "Expected bootstrap shard account to be populated.");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static async Task TestShardDefinitionStateStoreAsync()
    {
        string appRoot = CreateTempDirectory();
        try
        {
            LauncherPaths paths = new()
            {
                DataRoot = appRoot,
                StateFilePath = Path.Combine(appRoot, "launcher-state.json"),
                ActiveShardStateFilePath = Path.Combine(appRoot, "active-shard-state.json"),
                ShardStateDirectory = Path.Combine(appRoot, "shards"),
                ShardManifestPath = Path.Combine(appRoot, "shards.json"),
                ProtectedLicenseSecretsPath = Path.Combine(appRoot, "license-secrets.bin")
            };

            ShardDefinitionStateStore store = new(paths);

            ShardDefinition newRenaissance = new()
            {
                Id = "new-renaissance",
                Name = "New Renaissance",
                Host = "play.newrenaissanceuo.com",
                Account = "tester",
                Password = "secret-password",
                ServerPort = 2593,
                Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["displayGroup"] = "recommended",
                    [ShardMetadataKeys.ClientAssetPath] = @"C:\uo-assets"
                }
            };

            await store.SaveAsync(newRenaissance).ConfigureAwait(false);
            string newRenaissanceJson = await File.ReadAllTextAsync(store.GetPath("new-renaissance")).ConfigureAwait(false);
            Assert(!newRenaissanceJson.Contains("secret-password", StringComparison.Ordinal), "Plaintext passwords must not be persisted.");
            Assert(!newRenaissanceJson.Contains("displayGroup", StringComparison.Ordinal), "New Renaissance should not persist extra metadata.");
            Assert(newRenaissanceJson.Contains("client_asset_path", StringComparison.Ordinal), "Client asset path should persist when provided.");

            ShardDefinition rememberedNewRenaissance = store.ApplyRememberedState(new ShardDefinition
            {
                Id = "new-renaissance",
                Name = "New Renaissance",
                Host = "play.newrenaissanceuo.com",
                ServerPort = 2593,
                Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["displayGroup"] = "recommended"
                }
            });

            Assert(rememberedNewRenaissance.Account == "tester", "Expected remembered account to round-trip.");
            Assert(rememberedNewRenaissance.Password == "secret-password", "Expected remembered password to round-trip.");
            Assert(rememberedNewRenaissance.Metadata[ShardMetadataKeys.ClientAssetPath] == @"C:\uo-assets", "Expected client asset path to round-trip.");

            ShardDefinition newDawn = new()
            {
                Id = "uo-new-dawn",
                Name = "UO New Dawn",
                Host = "proxy.uonewdawn.com",
                Account = "newdawn-user-1",
                Password = "3pQw5br24L7mML8w",
                ServerPort = 2593,
                Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["refresh_token"] = true.ToString(),
                    ["jwt"] = "jwt-123",
                    ["discord_id"] = "123",
                    ["client_version"] = "1.0.0",
                    [ShardMetadataKeys.ClientAssetPath] = @"D:\uond-assets"
                }
            };

            await store.SaveAsync(newDawn).ConfigureAwait(false);
            string newDawnJson = await File.ReadAllTextAsync(store.GetPath("uo-new-dawn")).ConfigureAwait(false);
            Assert(!newDawnJson.Contains("3pQw5br24L7mML8w", StringComparison.Ordinal), "UO New Dawn password must be encrypted at rest.");
            Assert(newDawnJson.Contains("refresh_token", StringComparison.Ordinal), "Expected UO New Dawn refresh token preference to persist.");
            Assert(newDawnJson.Contains("client_asset_path", StringComparison.Ordinal), "Expected shared client asset path persistence.");
            Assert(!newDawnJson.Contains("jwt-123", StringComparison.Ordinal), "JWT metadata must not be persisted.");

            ShardDefinition rememberedNewDawn = store.ApplyRememberedState(new ShardDefinition
            {
                Id = "uo-new-dawn",
                Name = "UO New Dawn",
                Host = "proxy.uonewdawn.com",
                ServerPort = 2593,
                Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["payload_url_primary"] = "https://primary.example/payload.json"
                }
            });

            Assert(rememberedNewDawn.Account == "newdawn-user-1", "Expected remembered UO New Dawn account.");
            Assert(rememberedNewDawn.Password == "3pQw5br24L7mML8w", "Expected remembered UO New Dawn password.");
            Assert(rememberedNewDawn.Metadata.ContainsKey("refresh_token"), "Expected remembered refresh token preference.");
            Assert(rememberedNewDawn.Metadata[ShardMetadataKeys.ClientAssetPath] == @"D:\uond-assets", "Expected remembered shared client asset path.");
            Assert(!rememberedNewDawn.Metadata.ContainsKey("jwt"), "JWT should not be restored from persisted state.");

            ShardDefinition tidesWithBlankAssetPath = new()
            {
                Id = "tides-of-power",
                Name = "Tides of Power",
                Host = "login.uotides.com",
                Account = "placeholder",
                Password = "placeholder-password",
                ServerPort = 2593,
                Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    [ShardMetadataKeys.ClientAssetPath] = string.Empty
                }
            };

            await store.SaveAsync(tidesWithBlankAssetPath).ConfigureAwait(false);
            string tidesJson = await File.ReadAllTextAsync(store.GetPath("tides-of-power")).ConfigureAwait(false);
            Assert(!tidesJson.Contains("client_asset_path", StringComparison.Ordinal), "Blank asset paths should not be persisted.");

            await store.DeleteAsync("uo-new-dawn").ConfigureAwait(false);
            Assert(!File.Exists(store.GetPath("uo-new-dawn")), "Expected remembered state deletion when remember is disabled.");
        }
        finally
        {
            Directory.Delete(appRoot, recursive: true);
        }
    }

    private static async Task TestActiveShardStateRoundTripAsync()
    {
        string appRoot = CreateTempDirectory();
        try
        {
            LauncherPaths paths = new()
            {
                DataRoot = appRoot,
                StateFilePath = Path.Combine(appRoot, "launcher-state.json"),
                ActiveShardStateFilePath = Path.Combine(appRoot, "active-shard-state.json"),
                ShardStateDirectory = Path.Combine(appRoot, "shards"),
                ShardManifestPath = Path.Combine(appRoot, "shards.json"),
                ProtectedLicenseSecretsPath = Path.Combine(appRoot, "license-secrets.bin")
            };

            LauncherStateStore store = new(paths);
            LauncherState state = new()
            {
                SelectedShardId = "new-renaissance"
            };

            ActiveShardState active = ActiveShardStateFactory.Create(new ShardDefinition
            {
                Id = "new-renaissance",
                Name = "New Renaissance",
                Host = "play.newrenaissanceuo.com",
                Account = "tester",
                Password = "secret",
                ClientVersion = new Version(0, 0, 0, 0),
                ServerIP = IPAddress.Parse("203.0.113.10"),
                ServerPort = 2593,
                Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["displayGroup"] = "recommended"
                }
            });

            await store.SaveAsync(state).ConfigureAwait(false);
            await store.SaveActiveShardStateAsync(active).ConfigureAwait(false);

            LauncherState loadedState = await store.LoadAsync().ConfigureAwait(false);
            ActiveShardState? loadedActive = await store.LoadActiveShardStateAsync().ConfigureAwait(false);
            string launcherStateJson = await File.ReadAllTextAsync(paths.StateFilePath).ConfigureAwait(false);
            Assert(loadedState.SelectedShardId == state.SelectedShardId, "Selected shard id did not round-trip.");
            Assert(!launcherStateJson.Contains("licenseKey", StringComparison.OrdinalIgnoreCase), "launcher-state.json should not store a raw license key.");
            Assert(loadedActive is not null, "Active state was not loaded.");
            Assert(loadedActive!.SchemaVersion == 3, "Expected schema version 3.");
            Assert(loadedActive.Runtime.Account == "tester", "Active state account mismatch.");
            Assert(loadedActive.Runtime.ServerIP!.ToString() == "203.0.113.10", "Resolved IP did not round-trip.");

            ProtectedLicenseSecretStore secretStore = new(paths);
            await secretStore.SaveAsync(new ProtectedLicenseSecrets
            {
                LicenseKey = "UOAIO-SECRET-KEY",
                SessionToken = "session-token",
                MachineBinding = "machine-binding",
                TrustedTimeSnapshot = new TrustedTimeSnapshot
                {
                    TrustedUtc = DateTimeOffset.UtcNow,
                    TickCount64 = Environment.TickCount64,
                    LastResponseId = "resp-1"
                }
            }).ConfigureAwait(false);

            byte[] protectedBytes = await File.ReadAllBytesAsync(paths.ProtectedLicenseSecretsPath).ConfigureAwait(false);
            string secretFileText = Encoding.UTF8.GetString(protectedBytes);
            Assert(!secretFileText.Contains("UOAIO-SECRET-KEY", StringComparison.Ordinal), "Protected license storage must not contain plaintext secrets.");

            ProtectedLicenseSecrets? loadedSecrets = await secretStore.LoadAsync().ConfigureAwait(false);
            Assert(loadedSecrets is not null && loadedSecrets.LicenseKey == "UOAIO-SECRET-KEY", "Protected license key did not round-trip.");

            await File.WriteAllBytesAsync(paths.ProtectedLicenseSecretsPath, new byte[] { 0x01, 0x02, 0x03 }).ConfigureAwait(false);
            bool threw = false;
            try
            {
                await secretStore.LoadAsync().ConfigureAwait(false);
            }
            catch
            {
                threw = true;
            }

            Assert(threw, "Corrupted protected secret storage should fail closed.");
        }
        finally
        {
            Directory.Delete(appRoot, recursive: true);
        }
    }

    private static async Task TestClientBootstrapRoundTripAsync()
    {
        string appRoot = CreateTempDirectory();
        try
        {
            LauncherPaths paths = new()
            {
                DataRoot = appRoot,
                StateFilePath = Path.Combine(appRoot, "launcher-state.json"),
                ActiveShardStateFilePath = Path.Combine(appRoot, "active-shard-state.json"),
                ShardStateDirectory = Path.Combine(appRoot, "shards"),
                ShardManifestPath = Path.Combine(appRoot, "shards.json"),
                ProtectedLicenseSecretsPath = Path.Combine(appRoot, "license-secrets.bin")
            };

            ShardDefinition shard = new()
            {
                Id = "uo-new-dawn",
                Name = "UO New Dawn",
                Description = "Bootstrap test shard",
                Host = "proxy.uonewdawn.com",
                Account = "tester",
                Password = "secret",
                ClientVersion = new Version(1, 0, 0, 0),
                ServerIP = IPAddress.Parse("203.0.113.15"),
                ServerPort = 2593,
                Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["displayGroup"] = "recommended",
                    ["jwt"] = "jwt-123",
                    [ShardMetadataKeys.ClientAssetPath] = @"C:\uond-assets"
                }
            };

            ClientBootstrapDefinition bootstrap = ClientBootstrapDefinitionFactory.Create(shard);
            string pipeName = ClientBootstrapPipeTransport.CreatePipeName();
            Task<string> payloadTask = ReadPipePayloadAsync(pipeName);
            ClientBootstrapPipeTransport transport = new();
            await transport.WriteAsync(pipeName, bootstrap).ConfigureAwait(false);
            string payload = await payloadTask.ConfigureAwait(false);

            ClientBootstrapDefinition roundTripped = ClientBootstrapSerializer.Deserialize(payload);
            using JsonDocument document = JsonDocument.Parse(payload);
            JsonElement shardElement = document.RootElement.GetProperty("shard");
            Assert(document.RootElement.GetProperty("schemaVersion").GetInt32() == 2, "Expected bootstrap schema version 2.");
            Assert(shardElement.GetProperty("id").GetString() == "uo-new-dawn", "Expected shard id to round-trip.");
            Assert(shardElement.GetProperty("host").GetString() == "proxy.uonewdawn.com", "Expected shard host to round-trip.");
            Assert(shardElement.GetProperty("serverIp").GetString() == "203.0.113.15", "Expected resolved IP to round-trip.");
            Assert(shardElement.GetProperty("metadata").GetProperty("jwt").GetString() == "jwt-123", "Expected JWT metadata to round-trip.");
            Assert(shardElement.GetProperty("metadata").GetProperty("client_asset_path").GetString() == @"C:\uond-assets", "Expected client asset path metadata to round-trip.");
            Assert(roundTripped.Shard.ServerIP!.ToString() == "203.0.113.15", "Expected shared bootstrap serializer to restore the resolved IP.");
            Assert(roundTripped.Shard.Metadata[ShardMetadataKeys.ClientAssetPath] == @"C:\uond-assets", "Expected client asset path metadata to deserialize.");

            string clientExePath = Path.Combine(appRoot, "Ultima.Client.Host.exe");
            string dependencyPath = Path.Combine(appRoot, "Ultima.Client.dll");
            await File.WriteAllTextAsync(clientExePath, "stub-host").ConfigureAwait(false);
            await File.WriteAllTextAsync(dependencyPath, "stub-client").ConfigureAwait(false);

            string runtimeRoot = Path.Combine(appRoot, "runtime-root");
            Environment.SetEnvironmentVariable("UOAIO_CLIENT_RUNTIME_ROOT", runtimeRoot);
            string sessionRoot = Path.Combine(runtimeRoot, "sessions");
            Directory.CreateDirectory(sessionRoot);
            string expiredSessionPath = Path.Combine(sessionRoot, "expired-test-session-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(expiredSessionPath);
            Directory.SetLastWriteTimeUtc(expiredSessionPath, DateTime.UtcNow.AddHours(-30));

            ClientProcessLauncher launcher = new();
            ProcessStartInfo startInfo = launcher.CreateStartInfo(appRoot, pipeName);
            string resolvedExecutablePath = Path.GetFullPath(startInfo.FileName);
            string runtimeDataRoot = ExtractCommandArgument(startInfo.Arguments, "--runtime-data-root");
            Assert(resolvedExecutablePath == Path.GetFullPath(clientExePath), "Expected launcher to execute the client host in place.");
            Assert(Directory.Exists(runtimeDataRoot), "Expected runtime session data directory to be created.");
            Assert(startInfo.Arguments.Contains("--bootstrap-pipe", StringComparison.Ordinal), "Expected bootstrap pipe argument.");
            Assert(startInfo.Arguments.Contains(pipeName, StringComparison.Ordinal), "Expected bootstrap pipe name in arguments.");
            Assert(startInfo.Arguments.Contains("--runtime-data-root", StringComparison.Ordinal), "Expected runtime data root argument.");
            Assert(Path.GetFullPath(startInfo.WorkingDirectory!) == Path.GetFullPath(appRoot), "Expected app-root working directory.");
            Assert(!Directory.Exists(expiredSessionPath), "Expected expired staged sessions to be pruned.");
            Directory.Delete(runtimeDataRoot, recursive: true);
        }
        finally
        {
            Environment.SetEnvironmentVariable("UOAIO_CLIENT_RUNTIME_ROOT", null);
            Directory.Delete(appRoot, recursive: true);
        }
    }

    private static async Task TestUoNewDawnAuthorizationServiceAsync()
    {
        string root = CreateTempDirectory();
        try
        {
            ShardDefinition shard = new()
            {
                Id = "uo-new-dawn",
                Name = "UO New Dawn",
                Host = "proxy.uonewdawn.com",
                ServerPort = 2593,
                Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["payload_url_primary"] = "https://primary.example/payload.json",
                    ["payload_url_fallback"] = "https://fallback.example/payload.json",
                    ["oauth_callback_prefix"] = "http://localhost:4512/callback/"
                }
            };

            string token = CreateJwt("123456789", "newdawn-user", DateTimeOffset.UtcNow.AddMinutes(30));
            TestUoNewDawnAuthorizationService service = new(root, token);
            UoNewDawnAuthorizationResult result = await service.AcquireAuthorizationAsync(shard).ConfigureAwait(false);
            Assert(service.BrowserLaunchCount == 1, "Expected the browser auth flow to be launched once.");
            Assert(result.IdentityHint == "newdawn-user", "Expected identity hint from the JWT.");
            Assert(result.Metadata["jwt"] == token, "Expected JWT field to be captured.");
            Assert(result.Metadata["discord_id"] == "123456789", "Expected discord_id to be captured.");
            Assert(result.LoginHost == "proxy.uonewdawn.com", "Expected UO New Dawn login server host.");

            UoNewDawnAuthorizationResult cachedResult = await service.AcquireAuthorizationAsync(shard).ConfigureAwait(false);
            Assert(service.BrowserLaunchCount == 1, "Expected cached token to avoid re-opening the browser.");
            Assert(cachedResult.IdentityHint == "newdawn-user", "Expected cached identity hint.");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static Task TestUoNewDawnWorkflowStateMachineAsync()
    {
        UoNewDawnWorkflowState state = new();
        Assert(state.CurrentStage == UoNewDawnWorkflowStage.Authorize, "Expected auth stage initially.");
        Assert(!state.CanGoForward, "Should not be able to move forward before authorization.");

        state.RestoreSelectedAccount("first-user-2");
        state.RefreshToken = true;
        state.ApplyAuthorization(new UoNewDawnAuthorizationResult
        {
            IdentityHint = "first-user",
            LoginHost = "proxy.uonewdawn.com",
            LoginPort = 2593,
            Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["jwt"] = "jwt-1",
                ["discord_user"] = "first-user"
            }
        });

        Assert(state.CurrentStage == UoNewDawnWorkflowStage.SelectAccount, "Expected account stage after authorization.");
        Assert(state.AccountOptions.Count == 3, "Expected synthetic account list.");
        Assert(state.SelectedAccount == "first-user-2", "Expected restored account selection to survive into computed accounts.");

        state.GoBack();
        Assert(state.CurrentStage == UoNewDawnWorkflowStage.Authorize, "Expected back navigation to return to auth stage.");
        Assert(state.CanGoForward, "Expected forward navigation after successful authorization.");
        state.GoForward();
        Assert(state.CurrentStage == UoNewDawnWorkflowStage.SelectAccount, "Expected forward navigation to restore account stage.");

        state.ApplyAuthorization(new UoNewDawnAuthorizationResult
        {
            IdentityHint = "second-user",
            LoginHost = "proxy.uonewdawn.com",
            LoginPort = 2593,
            Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["jwt"] = "jwt-2",
                ["discord_user"] = "second-user"
            }
        });

        Assert(state.SelectedAccount == "second-user-1", "Expected account selection to reset when upstream auth data changes.");
        Dictionary<string, string> runtimeMetadata = state.BuildRuntimeMetadata("second-user-3");
        Assert(runtimeMetadata["account"] == "second-user-3", "Expected selected account in runtime metadata.");
        Assert(runtimeMetadata["username"] == "second-user-3", "Expected selected account to populate username metadata.");
        Assert(runtimeMetadata["password"] == "3pQw5br24L7mML8w", "Expected the legacy New Dawn password to be present for client packet login.");
        Assert(runtimeMetadata["jwt"] == "jwt-2", "Expected latest auth metadata in runtime metadata.");
        return Task.CompletedTask;
    }

    private static async Task TestLicenseClientAsync()
    {
        using RSA rsa = RSA.Create(2048);
        string publicPem = rsa.ExportRSAPublicKeyPem();

        TestHttpMessageHandler handler = new(async request =>
        {
            string body = await request.Content!.ReadAsStringAsync().ConfigureAwait(false);
            using JsonDocument bodyDocument = JsonDocument.Parse(body);
            string clientNonce = bodyDocument.RootElement.GetProperty("clientNonce").GetString() ?? string.Empty;
            string? licenseKey = bodyDocument.RootElement.TryGetProperty("licenseKey", out JsonElement licenseElement)
                ? licenseElement.GetString()
                : null;

            if (string.Equals(licenseKey, "bad-key", StringComparison.Ordinal))
            {
                return new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    Content = new StringContent("{\"error\":\"forbidden\"}", Encoding.UTF8, "application/json")
                };
            }

            if (request.RequestUri!.AbsolutePath.Contains("/launcher/session/start", StringComparison.Ordinal))
            {
                string echoedNonce = string.Equals(licenseKey, "nonce-bad", StringComparison.Ordinal) ? "wrong-nonce" : clientNonce;
                string payload = CreateSignedLicenseEnvelopeJson(rsa, new
                {
                    responseId = Guid.NewGuid().ToString("N"),
                    licenseId = "lic-1",
                    status = "active",
                    productCode = LauncherDefaults.ProductCode,
                    machineBinding = "hwid",
                    issuedAtUtc = "2026-05-11T00:00:00Z",
                    notBeforeUtc = "2026-05-11T00:00:00Z",
                    expiresAtUtc = "2026-05-12T00:00:00Z",
                    serverTimeUtc = "2026-05-11T00:00:00Z",
                    serverNonce = "srv-1",
                    clientNonce = echoedNonce,
                    sessionToken = "jwt-123",
                    sessionTokenId = "act-1",
                    policy = new
                    {
                        maxDevices = 3,
                        heartbeatMinutes = 15,
                        graceHours = 12,
                        resetAllowance = 5,
                        resetUsed = 1,
                        offlineTokenAllow = true
                    }
                });

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                };
            }

            if (request.RequestUri.AbsolutePath.Contains("/machines/heartbeat", StringComparison.Ordinal))
            {
                string payload = CreateSignedLicenseEnvelopeJson(rsa, new
                {
                    responseId = Guid.NewGuid().ToString("N"),
                    licenseId = "lic-1",
                    status = "active",
                    productCode = LauncherDefaults.ProductCode,
                    machineBinding = "hwid",
                    issuedAtUtc = "2026-05-11T00:05:00Z",
                    notBeforeUtc = "2026-05-11T00:05:00Z",
                    expiresAtUtc = "2026-05-12T00:00:00Z",
                    serverTimeUtc = "2026-05-11T00:05:00Z",
                    serverNonce = "srv-2",
                    clientNonce = clientNonce,
                    sessionToken = "jwt-123",
                    sessionTokenId = "act-1",
                    policy = new
                    {
                        maxDevices = 3,
                        heartbeatMinutes = 15,
                        graceHours = 12,
                        resetAllowance = 5,
                        resetUsed = 1,
                        offlineTokenAllow = true
                    }
                });

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(CreateSignedLicenseEnvelopeJson(rsa, new
                {
                    responseId = Guid.NewGuid().ToString("N"),
                    licenseId = "lic-1",
                    status = "active",
                    productCode = LauncherDefaults.ProductCode,
                    machineBinding = "hwid",
                    issuedAtUtc = "2026-05-11T00:00:00Z",
                    notBeforeUtc = "2026-05-11T00:00:00Z",
                    expiresAtUtc = "2026-05-11T12:00:00Z",
                    serverTimeUtc = "2026-05-11T00:00:00Z",
                    serverNonce = "srv-3",
                    clientNonce = clientNonce,
                    offlineToken = "offline-123",
                    sessionTokenId = "act-1",
                    policy = new
                    {
                        maxDevices = 3,
                        heartbeatMinutes = 15,
                        graceHours = 12,
                        resetAllowance = 5,
                        resetUsed = 1,
                        offlineTokenAllow = true
                    }
                }), Encoding.UTF8, "application/json")
            };
        });

        LicenseTrustSettings trust = new()
        {
            ProductionLicenseServerUrl = "https://licenses.example.invalid",
            SigningPublicKeyPem = publicPem,
            AllowDeveloperOverride = true
        };
        LauncherLicenseClient client = new(new HttpClient(handler), trust);
        List<ReplayCacheEntry> replayCache = new();

        LicenseVerificationResult session = await client.StartSessionAsync(new Uri("http://localhost:8080"), "good-key", "hwid", "machine", replayCache, null).ConfigureAwait(false);
        Assert(session.SessionToken == "jwt-123", "License client did not parse the session token.");
        Assert(session.Summary.Policy.OfflineTokenAllowed, "License policy flag did not parse.");

        OfflineLicenseGrant offlineGrant = await client.IssueOfflineGrantAsync(new Uri("http://localhost:8080"), session.SessionToken, "hwid", replayCache, session.TrustedTimeSnapshot).ConfigureAwait(false);
        Assert(offlineGrant.OfflineToken == "offline-123", "Offline grant did not round-trip.");

        LicenseVerificationResult heartbeat = await client.HeartbeatAsync(new Uri("http://localhost:8080"), "good-key", session.SessionToken, "hwid", replayCache, session.TrustedTimeSnapshot).ConfigureAwait(false);
        Assert(heartbeat.Summary.Status == "active", "Heartbeat did not return an active state.");

        bool threw = false;
        try
        {
            await client.StartSessionAsync(new Uri("http://localhost:8080"), "bad-key", "hwid", "machine", replayCache, session.TrustedTimeSnapshot).ConfigureAwait(false);
        }
        catch (LicenseValidationException)
        {
            threw = true;
        }

        Assert(threw, "Expected invalid license flow to throw.");

        threw = false;
        try
        {
            await client.StartSessionAsync(new Uri("http://localhost:8080"), "nonce-bad", "hwid", "machine", replayCache, session.TrustedTimeSnapshot).ConfigureAwait(false);
        }
        catch (LicenseValidationException)
        {
            threw = true;
        }

        Assert(threw, "Expected mismatched nonce flow to throw.");

        using RSA wrongRsa = RSA.Create(2048);
        LauncherLicenseClient wrongKeyClient = new(new HttpClient(handler), new LicenseTrustSettings
        {
            ProductionLicenseServerUrl = "https://licenses.example.invalid",
            SigningPublicKeyPem = wrongRsa.ExportRSAPublicKeyPem(),
            AllowDeveloperOverride = true
        });

        threw = false;
        try
        {
            await wrongKeyClient.StartSessionAsync(new Uri("http://localhost:8080"), "good-key", "hwid", "machine", replayCache, session.TrustedTimeSnapshot).ConfigureAwait(false);
        }
        catch (LicenseValidationException)
        {
            threw = true;
        }

        Assert(threw, "Expected wrong signing key flow to throw.");
    }

    private static async Task TestManifestVerificationAsync()
    {
        using RSA rsa = RSA.Create(2048);
        string publicPem = rsa.ExportRSAPublicKeyPem();
        ReleaseManifest manifest = new()
        {
            Channel = "stable",
            Packages = new List<ReleasePackage>
            {
                new() { PackageId = "launcher", Version = "1.0.0", DownloadUrl = "launcher.zip", Sha256 = "ABC" }
            }
        };

        byte[] bytes = ReleaseManifestSerializer.GetManifestBytes(manifest);
        byte[] signature = rsa.SignData(bytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        SignedReleaseManifest envelope = new()
        {
            Manifest = manifest,
            SignatureBase64 = Convert.ToBase64String(signature)
        };

        ReleaseManifestVerifier verifier = new();
        verifier.Verify(envelope, publicPem);
        await Task.CompletedTask;
    }

    private static async Task TestUpdateCoordinatorAsync()
    {
        string root = CreateTempDirectory();
        try
        {
            using RSA rsa = RSA.Create(2048);
            string publicPem = rsa.ExportRSAPublicKeyPem();
            string publicKeyPath = Path.Combine(root, "release-public.pem");
            await File.WriteAllTextAsync(publicKeyPath, publicPem).ConfigureAwait(false);

            string packageV1Zip = CreateZipPackage(root, "client-v1.zip", "version.txt", "1.0.0");
            string packageV2Zip = CreateZipPackage(root, "client-v2.zip", "version.txt", "2.0.0");

            string manifestPath = Path.Combine(root, "manifest.json");
            await WriteSignedManifestAsync(rsa, manifestPath, new ReleaseManifest
            {
                Channel = "stable",
                Packages = new List<ReleasePackage>
                {
                    new()
                    {
                        PackageId = "client",
                        Version = "1.0.0",
                        DownloadUrl = new Uri(packageV1Zip).AbsoluteUri,
                        Sha256 = ReleaseManifestSerializer.ComputeSha256(packageV1Zip),
                        EntryPoint = "version.txt"
                    }
                }
            }).ConfigureAwait(false);

            BootstrapperSettings settings = new()
            {
                ManifestUri = new Uri(manifestPath).AbsoluteUri,
                InstallRoot = Path.Combine(root, "install"),
                PublicKeyPemPath = publicKeyPath,
                Channel = "stable"
            };

            UpdateCoordinator coordinator = new(settings, new ReleaseManifestVerifier());
            UpdateCheckResult firstCheck = await coordinator.CheckForUpdatesAsync().ConfigureAwait(false);
            Assert(firstCheck.HasUpdates, "Expected first update check to find a package.");
            StagedUpdatePlan firstPlan = await coordinator.StageUpdatesAsync(firstCheck).ConfigureAwait(false);
            await coordinator.ApplyUpdatesAsync(firstPlan).ConfigureAwait(false);

            string installedVersionPath = Path.Combine(settings.InstallRoot, "current", "client", "version.txt");
            Assert(await File.ReadAllTextAsync(installedVersionPath).ConfigureAwait(false) == "1.0.0", "Expected version 1.0.0 after first apply.");

            await WriteSignedManifestAsync(rsa, manifestPath, new ReleaseManifest
            {
                Channel = "stable",
                Packages = new List<ReleasePackage>
                {
                    new()
                    {
                        PackageId = "client",
                        Version = "2.0.0",
                        DownloadUrl = new Uri(packageV2Zip).AbsoluteUri,
                        Sha256 = ReleaseManifestSerializer.ComputeSha256(packageV2Zip),
                        EntryPoint = "version.txt"
                    }
                }
            }).ConfigureAwait(false);

            UpdateCheckResult secondCheck = await coordinator.CheckForUpdatesAsync().ConfigureAwait(false);
            StagedUpdatePlan secondPlan = await coordinator.StageUpdatesAsync(secondCheck).ConfigureAwait(false);
            await coordinator.ApplyUpdatesAsync(secondPlan).ConfigureAwait(false);
            Assert(await File.ReadAllTextAsync(installedVersionPath).ConfigureAwait(false) == "2.0.0", "Expected version 2.0.0 after second apply.");

            await coordinator.RollbackAsync().ConfigureAwait(false);
            Assert(await File.ReadAllTextAsync(installedVersionPath).ConfigureAwait(false) == "1.0.0", "Expected rollback to restore version 1.0.0.");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static async Task WriteSignedManifestAsync(RSA rsa, string path, ReleaseManifest manifest)
    {
        byte[] bytes = ReleaseManifestSerializer.GetManifestBytes(manifest);
        byte[] signature = rsa.SignData(bytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        SignedReleaseManifest envelope = new()
        {
            Manifest = manifest,
            SignatureBase64 = Convert.ToBase64String(signature)
        };

        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(envelope, JsonOptions)).ConfigureAwait(false);
    }

    private static string CreateSignedLicenseEnvelopeJson<T>(RSA rsa, T payload)
    {
        JsonSerializerOptions compactOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = false };
        JsonElement payloadElement = JsonSerializer.SerializeToElement(payload, compactOptions);
        string payloadJson = payloadElement.GetRawText();
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payloadJson);
        byte[] signature = rsa.SignData(payloadBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return JsonSerializer.Serialize(new
        {
            payload = payloadElement,
            signatureBase64 = Convert.ToBase64String(signature),
            algorithm = "RS256",
            keyId = LauncherDefaults.LicenseSigningKeyId
        }, compactOptions);
    }

    private static string CreateZipPackage(string root, string zipName, string fileName, string contents)
    {
        string staging = Path.Combine(root, Path.GetFileNameWithoutExtension(zipName));
        Directory.CreateDirectory(staging);
        File.WriteAllText(Path.Combine(staging, fileName), contents);
        string zipPath = Path.Combine(root, zipName);
        if (File.Exists(zipPath))
        {
            File.Delete(zipPath);
        }

        System.IO.Compression.ZipFile.CreateFromDirectory(staging, zipPath);
        return zipPath;
    }

    private static string CreateTempDirectory()
    {
        string path = Path.Combine(Path.GetTempPath(), "uoaio-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static string ExtractCommandArgument(string arguments, string optionName)
    {
        string marker = optionName + " \"";
        int start = arguments.IndexOf(marker, StringComparison.Ordinal);
        if (start < 0)
        {
            throw new InvalidOperationException($"Unable to find argument '{optionName}' in '{arguments}'.");
        }

        start += marker.Length;
        int end = arguments.IndexOf('"', start);
        if (end < 0)
        {
            throw new InvalidOperationException($"Unable to parse quoted value for argument '{optionName}'.");
        }

        return arguments.Substring(start, end - start);
    }

    private static string CreateJwt(string discordId, string discordUser, DateTimeOffset expiresAtUtc)
    {
        string header = Base64UrlEncode("{\"alg\":\"none\",\"typ\":\"JWT\"}");
        string payload = Base64UrlEncode($"{{\"discord_id\":\"{discordId}\",\"discord_user\":\"{discordUser}\",\"exp\":{expiresAtUtc.ToUnixTimeSeconds()}}}");
        return $"{header}.{payload}.signature";
    }

    private static string Base64UrlEncode(string value)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(value)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static async Task<string> ReadPipePayloadAsync(string pipeName)
    {
        using NamedPipeClientStream pipe = new(".", pipeName, PipeDirection.In, PipeOptions.Asynchronous);
        await pipe.ConnectAsync().ConfigureAwait(false);
        using StreamReader reader = new(pipe, Encoding.UTF8, leaveOpen: false);
        return await reader.ReadToEndAsync().ConfigureAwait(false);
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}

internal sealed class TestHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _handler;

    public TestHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
    {
        _handler = handler;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return _handler(request);
    }
}

internal sealed class TestUoNewDawnAuthorizationService : UoNewDawnAuthorizationService
{
    private readonly string _cacheRoot;
    private readonly string _token;

    public TestUoNewDawnAuthorizationService(string cacheRoot, string token)
    {
        _cacheRoot = cacheRoot;
        _token = token;
    }

    public int BrowserLaunchCount { get; private set; }

    protected override Task<UoNewDawnPayload> FetchPayloadWithFallbackAsync(ShardDefinition shard, CancellationToken cancellationToken)
    {
        return Task.FromResult(new UoNewDawnPayload
        {
            Config = new UoNewDawnLauncherConfig
            {
                ClientVersion = "1.0.0",
                LoginUrl = "https://login.uonewdawn.com",
                LoginServers = new List<UoNewDawnLoginServer>
                {
                    new() { Name = "Proxy", Host = "proxy.uonewdawn.com", Port = 2593 }
                }
            }
        });
    }

    protected override Task LaunchBrowserAsync(string authUrl, CancellationToken cancellationToken)
    {
        BrowserLaunchCount++;
        return Task.CompletedTask;
    }

    protected override Task<string> ListenForCallbackAsync(string callbackPrefix, string successRedirectUrl, CancellationToken cancellationToken)
    {
        return Task.FromResult(_token);
    }

    protected override string GetTokenCachePath()
    {
        Directory.CreateDirectory(_cacheRoot);
        return Path.Combine(_cacheRoot, "uond-test-token.cache");
    }
}
