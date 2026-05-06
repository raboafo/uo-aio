using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using UOAIO.ShardRuntime;

namespace UOAIO.Launcher.Core;

public sealed class UoNewDawnAuthorizationResult
{
    public string IdentityHint { get; set; } = string.Empty;

    public string LoginHost { get; set; } = string.Empty;

    public int LoginPort { get; set; }

    public Dictionary<string, string> Metadata { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public enum UoNewDawnWorkflowStage
{
    Authorize = 0,
    SelectAccount = 1
}

public sealed class UoNewDawnWorkflowState
{
    private readonly List<string> _accountOptions = new();
    private string _preferredSelectedAccount = string.Empty;

    public UoNewDawnWorkflowStage CurrentStage { get; private set; } = UoNewDawnWorkflowStage.Authorize;

    public bool RefreshToken { get; set; }

    public UoNewDawnAuthorizationResult? Authorization { get; private set; }

    public IReadOnlyList<string> AccountOptions => _accountOptions;

    public string SelectedAccount { get; private set; } = string.Empty;

    public bool HasAuthorization => Authorization is not null;

    public bool CanGoBack => CurrentStage == UoNewDawnWorkflowStage.SelectAccount;

    public bool CanGoForward => CurrentStage == UoNewDawnWorkflowStage.Authorize && HasAuthorization;

    public void ApplyAuthorization(UoNewDawnAuthorizationResult authorization)
    {
        Authorization = authorization ?? throw new ArgumentNullException(nameof(authorization));
        RecomputeAccounts();
        CurrentStage = UoNewDawnWorkflowStage.SelectAccount;
    }

    public void GoBack()
    {
        if (CanGoBack)
        {
            CurrentStage = UoNewDawnWorkflowStage.Authorize;
        }
    }

    public void GoForward()
    {
        if (CanGoForward)
        {
            CurrentStage = UoNewDawnWorkflowStage.SelectAccount;
        }
    }

    public void RestoreSelectedAccount(string? selectedAccount)
    {
        if (string.IsNullOrWhiteSpace(selectedAccount))
        {
            return;
        }

        _preferredSelectedAccount = selectedAccount.Trim();
        SelectedAccount = _preferredSelectedAccount;
        RecomputeAccounts();
    }

    public void SetSelectedAccount(string? account)
    {
        string candidate = account?.Trim() ?? string.Empty;
        if (_accountOptions.Contains(candidate, StringComparer.OrdinalIgnoreCase))
        {
            SelectedAccount = _accountOptions.First(option => string.Equals(option, candidate, StringComparison.OrdinalIgnoreCase));
            _preferredSelectedAccount = SelectedAccount;
        }
    }

    public Dictionary<string, string> BuildRuntimeMetadata(string selectedAccount)
    {
        if (Authorization is null)
        {
            throw new InvalidOperationException("Authorization has not completed yet.");
        }

        SetSelectedAccount(selectedAccount);
        if (string.IsNullOrWhiteSpace(SelectedAccount))
        {
            throw new InvalidOperationException("An account must be selected before continuing.");
        }

        Dictionary<string, string> metadata = new(Authorization.Metadata, StringComparer.OrdinalIgnoreCase)
        {
            ["username"] = SelectedAccount,
            ["password"] = "3pQw5br24L7mML8w",
            ["account"] = SelectedAccount,
            ["selected_account"] = SelectedAccount
        };

        return metadata;
    }

    private void RecomputeAccounts()
    {
        _accountOptions.Clear();
        if (Authorization is null)
        {
            SelectedAccount = string.Empty;
            return;
        }

        string displayName = string.IsNullOrWhiteSpace(Authorization.IdentityHint) ? "Account" : Authorization.IdentityHint;
        _accountOptions.Add($"{displayName}-1");
        _accountOptions.Add($"{displayName}-2");
        _accountOptions.Add($"{displayName}-3");

        string preferred = string.IsNullOrWhiteSpace(SelectedAccount) ? _preferredSelectedAccount : SelectedAccount;
        if (_accountOptions.Contains(preferred, StringComparer.OrdinalIgnoreCase))
        {
            SelectedAccount = _accountOptions.First(option => string.Equals(option, preferred, StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            SelectedAccount = _accountOptions[0];
        }

        _preferredSelectedAccount = SelectedAccount;
    }
}

public class UoNewDawnAuthorizationService
{
    public const string ShardId = "uo-new-dawn";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ValidationResult ValidateShard(ShardDefinition shard)
    {
        List<string> errors = new();
        if (!shard.Metadata.ContainsKey("payload_url_primary"))
        {
            errors.Add("UO New Dawn requires 'payload_url_primary' in shard metadata.");
        }

        return errors.Count == 0 ? ValidationResult.Success : ValidationResult.Failure(errors.ToArray());
    }

    public async Task<UoNewDawnAuthorizationResult> AcquireAuthorizationAsync(ShardDefinition shard, bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        ValidationResult validation = ValidateShard(shard);
        if (!validation.IsValid)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, validation.Errors));
        }

        UoNewDawnPayload payload = await FetchPayloadWithFallbackAsync(shard, cancellationToken).ConfigureAwait(false);
        UoNewDawnLoginServer loginServer = SelectLoginServer(payload);
        string callbackPrefix = GetMetadataOrDefault(shard, "oauth_callback_prefix", "http://localhost:4512/callback/");
        string loginUrl = NormalizeLoginUrl(payload.Config.LoginUrl);

        UoNewDawnJwtClaims? cached = forceRefresh ? null : await TryLoadCachedClaimsAsync(cancellationToken).ConfigureAwait(false);
        if (cached is not null)
        {
            return BuildResult(cached, payload.Config, loginServer, callbackPrefix);
        }

        await LaunchBrowserAsync($"{loginUrl}/auth", cancellationToken).ConfigureAwait(false);
        string token = await ListenForCallbackAsync(callbackPrefix, $"{loginUrl}/auth-success", cancellationToken).ConfigureAwait(false);
        UoNewDawnJwtClaims claims = UoNewDawnJwtClaims.Parse(token);
        if (string.IsNullOrWhiteSpace(claims.DiscordId) || string.IsNullOrWhiteSpace(claims.DiscordUser))
        {
            throw new InvalidOperationException("The UO New Dawn token did not contain discord_id and discord_user claims.");
        }

        await SaveTokenAsync(token, claims.ExpiresAtUtc, cancellationToken).ConfigureAwait(false);
        return BuildResult(claims with { Token = token }, payload.Config, loginServer, callbackPrefix);
    }

    protected virtual async Task<UoNewDawnPayload> FetchPayloadWithFallbackAsync(ShardDefinition shard, CancellationToken cancellationToken)
    {
        List<string> urls = new();
        if (shard.Metadata.TryGetValue("payload_url_primary", out string? primary) && !string.IsNullOrWhiteSpace(primary))
        {
            urls.Add(primary);
        }

        if (shard.Metadata.TryGetValue("payload_url_fallback", out string? fallback) && !string.IsNullOrWhiteSpace(fallback))
        {
            urls.Add(fallback);
        }

        Exception? lastError = null;
        foreach (string url in urls)
        {
            try
            {
                using HttpClient client = new();
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("UOAIOLauncher", "1.0"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                using HttpResponseMessage response = await client.GetAsync(url, cancellationToken).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                UoNewDawnPayload? payload = JsonSerializer.Deserialize<UoNewDawnPayload>(json, JsonOptions);
                if (payload?.Config is null)
                {
                    throw new InvalidOperationException("UO New Dawn payload is missing launcher config.");
                }

                return payload;
            }
            catch (Exception ex)
            {
                lastError = ex;
            }
        }

        throw new InvalidOperationException("Unable to retrieve the UO New Dawn payload manifest.", lastError);
    }

    protected virtual Task LaunchBrowserAsync(string authUrl, CancellationToken cancellationToken)
    {
        Process.Start(new ProcessStartInfo(authUrl)
        {
            UseShellExecute = true
        });
        return Task.CompletedTask;
    }

    protected virtual async Task<string> ListenForCallbackAsync(string callbackPrefix, string successRedirectUrl, CancellationToken cancellationToken)
    {
        using HttpListener listener = new();
        listener.Prefixes.Add(callbackPrefix);
        listener.Start();
        using CancellationTokenRegistration registration = cancellationToken.Register(() =>
        {
            try
            {
                listener.Stop();
            }
            catch
            {
            }
        });

        while (true)
        {
            HttpListenerContext context;
            try
            {
                context = await listener.GetContextAsync().ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is HttpListenerException || ex is ObjectDisposedException)
            {
                cancellationToken.ThrowIfCancellationRequested();
                throw;
            }

            if (!string.Equals(context.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = 405;
                context.Response.Close();
                continue;
            }

            string token = context.Request.QueryString["token"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(token))
            {
                context.Response.StatusCode = 400;
                context.Response.Close();
                continue;
            }

            byte[] responseBytes = Encoding.UTF8.GetBytes(BuildSuccessHtml(successRedirectUrl));
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.ContentLength64 = responseBytes.Length;
            await context.Response.OutputStream.WriteAsync(responseBytes, 0, responseBytes.Length, cancellationToken).ConfigureAwait(false);
            context.Response.Close();
            return token;
        }
    }

    protected virtual string GetTokenCachePath()
    {
        string root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UOAIO", "Launcher", "tokens");
        Directory.CreateDirectory(root);
        return Path.Combine(root, "uond-token.cache");
    }

    protected virtual async Task<UoNewDawnJwtClaims?> TryLoadCachedClaimsAsync(CancellationToken cancellationToken)
    {
        string path = GetTokenCachePath();
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            byte[] raw = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
            byte[] decrypted = UnprotectTokenBytes(raw);

            UoNewDawnTokenCacheData? data = JsonSerializer.Deserialize<UoNewDawnTokenCacheData>(Encoding.UTF8.GetString(decrypted), JsonOptions);
            if (data is null || string.IsNullOrWhiteSpace(data.AccessToken))
            {
                return null;
            }

            UoNewDawnJwtClaims claims = UoNewDawnJwtClaims.Parse(data.AccessToken);
            if (claims.IsExpired)
            {
                TryDeleteCache(path);
                return null;
            }

            return claims;
        }
        catch
        {
            TryDeleteCache(path);
            return null;
        }
    }

    protected virtual async Task SaveTokenAsync(string token, DateTimeOffset expiresAtUtc, CancellationToken cancellationToken)
    {
        string path = GetTokenCachePath();
        UoNewDawnTokenCacheData cacheData = new()
        {
            AccessToken = token,
            ExpiresAtUtc = expiresAtUtc.UtcDateTime
        };

        byte[] json = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(cacheData, JsonOptions));
        byte[] protectedBytes = ProtectTokenBytes(json);
        await File.WriteAllBytesAsync(path, protectedBytes, cancellationToken).ConfigureAwait(false);
    }

    private static void TryDeleteCache(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
        }
    }

    private static string BuildSuccessHtml(string successRedirectUrl)
    {
        string encodedUrl = WebUtility.HtmlEncode(successRedirectUrl);
        return
            "<!DOCTYPE html>\n" +
            "<html>\n" +
            "<head>\n" +
            "    <title>Authorization Successful</title>\n" +
            $"    <meta http-equiv=\"refresh\" content=\"1;url={encodedUrl}\" />\n" +
            "    <style>\n" +
            "        body { background-color: #000; color: #f0f8ff; font-family: sans-serif; display: flex; flex-direction: column; align-items: center; justify-content: center; height: 100vh; margin: 0; padding: 20px; box-sizing: border-box; }\n" +
            "        p { font-size: 1.1em; margin-bottom: 12px; text-align: center; }\n" +
            "        a { color: #add8e6; text-decoration: none; }\n" +
            "    </style>\n" +
            "</head>\n" +
            "<body>\n" +
            "    <p>The UO New Dawn launcher has been authorized.</p>\n" +
            "    <p>You will be redirected automatically.</p>\n" +
            $"    <p><a href=\"{encodedUrl}\">Click here if you are not redirected.</a></p>\n" +
            "</body>\n" +
            "</html>";
    }

    private static string NormalizeLoginUrl(string loginUrl)
    {
        return loginUrl.EndsWith("/", StringComparison.Ordinal) ? loginUrl[..^1] : loginUrl;
    }

    private static UoNewDawnLoginServer SelectLoginServer(UoNewDawnPayload payload)
    {
        UoNewDawnLoginServer? selected = payload.Config.LoginServers
            .FirstOrDefault(server => !string.IsNullOrWhiteSpace(server.Host) && !server.Host.Contains("Live", StringComparison.OrdinalIgnoreCase))
            ?? payload.Config.LoginServers.FirstOrDefault(server => !string.IsNullOrWhiteSpace(server.Host));

        if (selected is null)
        {
            return new UoNewDawnLoginServer
            {
                Name = "Fallback",
                Host = "proxy.uonewdawn.com",
                Port = 2593
            };
        }

        return selected;
    }

    private static UoNewDawnAuthorizationResult BuildResult(UoNewDawnJwtClaims claims, UoNewDawnLauncherConfig config, UoNewDawnLoginServer loginServer, string callbackPrefix)
    {
        return new UoNewDawnAuthorizationResult
        {
            IdentityHint = claims.DiscordUser,
            LoginHost = loginServer.Host ?? string.Empty,
            LoginPort = loginServer.Port,
            Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["jwt"] = claims.Token,
                ["discord_id"] = claims.DiscordId,
                ["discord_user"] = claims.DiscordUser,
                ["login_url"] = NormalizeLoginUrl(config.LoginUrl),
                ["login_server_name"] = loginServer.Name ?? string.Empty,
                ["login_server_host"] = loginServer.Host ?? string.Empty,
                ["login_server_port"] = loginServer.Port.ToString(),
                ["client_version"] = config.ClientVersion ?? string.Empty,
                ["new_dawn_client_version"] = "1.5",
                ["oauth_callback_prefix"] = callbackPrefix
            }
        };
    }

    private static string GetMetadataOrDefault(ShardDefinition shard, string key, string fallback)
    {
        return shard.Metadata.TryGetValue(key, out string? value) && !string.IsNullOrWhiteSpace(value) ? value : fallback;
    }

    private static byte[] ProtectTokenBytes(byte[] plainBytes)
    {
        using Aes aes = Aes.Create();
        aes.Key = DeriveTokenCacheKey();
        aes.GenerateIV();

        using ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using MemoryStream output = new();
        output.WriteByte((byte)aes.IV.Length);
        output.Write(aes.IV, 0, aes.IV.Length);
        using (CryptoStream cryptoStream = new(output, encryptor, CryptoStreamMode.Write, leaveOpen: true))
        {
            cryptoStream.Write(plainBytes, 0, plainBytes.Length);
        }

        return output.ToArray();
    }

    private static byte[] UnprotectTokenBytes(byte[] protectedBytes)
    {
        if (protectedBytes.Length < 17)
        {
            return protectedBytes;
        }

        using MemoryStream input = new(protectedBytes);
        int ivLength = input.ReadByte();
        if (ivLength <= 0 || protectedBytes.Length <= 1 + ivLength)
        {
            return protectedBytes;
        }

        byte[] iv = new byte[ivLength];
        input.Read(iv, 0, iv.Length);

        using Aes aes = Aes.Create();
        aes.Key = DeriveTokenCacheKey();
        using ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, iv);
        using CryptoStream cryptoStream = new(input, decryptor, CryptoStreamMode.Read);
        using MemoryStream output = new();
        cryptoStream.CopyTo(output);
        return output.ToArray();
    }

    private static byte[] DeriveTokenCacheKey()
    {
        string material = $"{Environment.UserName}|{Environment.MachineName}|UOAIO-UOND-TOKEN-CACHE";
        return SHA256.HashData(Encoding.UTF8.GetBytes(material));
    }
}

public sealed class UoNewDawnPayload
{
    public UoNewDawnLauncherConfig Config { get; set; } = new();
}

public sealed class UoNewDawnLauncherConfig
{
    public string ClientVersion { get; set; } = string.Empty;

    public int ClientSource { get; set; }

    public List<UoNewDawnLoginServer> LoginServers { get; set; } = new();

    public string LoginUrl { get; set; } = string.Empty;
}

public sealed class UoNewDawnLoginServer
{
    public string Name { get; set; } = string.Empty;

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; }
}

public sealed class UoNewDawnTokenCacheData
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }
}

public sealed record UoNewDawnJwtClaims(string Token, string DiscordId, string DiscordUser, DateTimeOffset ExpiresAtUtc)
{
    public bool IsExpired => ExpiresAtUtc <= DateTimeOffset.UtcNow;

    public static UoNewDawnJwtClaims Parse(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("JWT token was empty.");
        }

        string[] parts = token.Split('.');
        if (parts.Length < 2)
        {
            throw new InvalidOperationException("JWT token was malformed.");
        }

        byte[] payloadBytes = DecodeBase64Url(parts[1]);
        using JsonDocument document = JsonDocument.Parse(payloadBytes);
        JsonElement root = document.RootElement;

        string discordId = root.TryGetProperty("discord_id", out JsonElement discordIdValue) ? discordIdValue.GetString() ?? string.Empty : string.Empty;
        string discordUser = root.TryGetProperty("discord_user", out JsonElement discordUserValue) ? discordUserValue.GetString() ?? string.Empty : string.Empty;
        long exp = root.TryGetProperty("exp", out JsonElement expValue) && expValue.TryGetInt64(out long expUnix)
            ? expUnix
            : 0;

        return new UoNewDawnJwtClaims(token, discordId, discordUser, exp > 0 ? DateTimeOffset.FromUnixTimeSeconds(exp) : DateTimeOffset.MinValue);
    }

    private static byte[] DecodeBase64Url(string value)
    {
        string normalized = value.Replace('-', '+').Replace('_', '/');
        switch (normalized.Length % 4)
        {
            case 2:
                normalized += "==";
                break;
            case 3:
                normalized += "=";
                break;
        }

        return Convert.FromBase64String(normalized);
    }
}
