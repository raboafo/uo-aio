using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UOAIO.Launcher.Core;
using UOAIO.Launcher.ShardWorkflows;
using UOAIO.ShardRuntime;

namespace UOAIO.Launcher;

public partial class MainWindow : Window
{
    private readonly HttpClient _httpClient = new();
    private readonly JsonSerializerOptions _previewOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters =
        {
            new IPAddressJsonConverter(),
            new VersionJsonConverter()
        }
    };
    private readonly ShardWorkflowRegistry<Func<IShardWorkflowControl>> _workflowRegistry = new(new[]
    {
        new ShardWorkflowRegistration<Func<IShardWorkflowControl>>("new-renaissance", static () => new NewRenaissanceWorkflowControl()),
        new ShardWorkflowRegistration<Func<IShardWorkflowControl>>("uo-new-dawn", static () => new UoNewDawnWorkflowControl())
    });

    private bool _isInitializing;
    private LauncherPaths? _paths;
    private LauncherStateStore? _stateStore;
    private ShardDefinitionStateStore? _shardStateStore;
    private LauncherState _state = new();
    private ShardCatalog _catalog = new();

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        _isInitializing = true;
        try
        {
            _paths = LauncherPaths.CreateDefault(AppContext.BaseDirectory);
            _stateStore = new LauncherStateStore(_paths);
            _shardStateStore = new ShardDefinitionStateStore(_paths);
            LauncherState state = await _stateStore.LoadAsync();
            _state = state;
            LicenseServerUrlTextBox.Text = string.IsNullOrWhiteSpace(state.LicenseServerUrl) ? LauncherDefaults.DefaultLicenseServerUrl : state.LicenseServerUrl;
            LicenseKeyTextBox.Text = state.LicenseKey;
            RememberInputsCheckBox.IsChecked = state.RememberInputs;
            ActiveStatePathTextBlock.Text = "named-pipe://not-prepared";

            ShardCatalogService catalogService = new();
            try
            {
                _catalog = await catalogService.LoadAsync(_paths.ShardManifestPath);
                ShardListBox.ItemsSource = _catalog.Shards;
                if (!string.IsNullOrWhiteSpace(_state.SelectedShardId))
                {
                    ShardDefinition? preferred = _catalog.Shards.FirstOrDefault(shard => string.Equals(shard.Id, _state.SelectedShardId, StringComparison.OrdinalIgnoreCase));
                    if (preferred is not null)
                    {
                        ShardListBox.SelectedItem = preferred;
                    }
                }

                if (ShardListBox.SelectedItem is null && _catalog.Shards.Count > 0)
                {
                    ShardListBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                ShowSaveStatus($"Failed to load shard catalog: {ex.Message}", isError: true);
            }

            ActiveStatePreviewTextBox.Text = "No client bootstrap has been prepared yet.";
            UpdateLicenseStatusFromState();
        }
        finally
        {
            _isInitializing = false;
        }
    }

    private void UpdateLicenseStatusFromState()
    {
        if (_state.LastLicenseSession is null)
        {
            ShowLicenseStatus("License has not been validated yet.", isError: false);
            return;
        }

        ShowLicenseStatus(
            $"Validated license {_state.LastLicenseSession.LicenseId} until {_state.LastLicenseSession.ExpiresAtUtc:u}. Session will be reused only while the license key and server URL remain unchanged.",
            isError: false);
    }

    private async void ValidateLicenseButton_Click(object sender, RoutedEventArgs e)
    {
        string serverUrl = LicenseServerUrlTextBox.Text.Trim();
        string licenseKey = LicenseKeyTextBox.Text.Trim();
        if (!Uri.TryCreate(serverUrl, UriKind.Absolute, out Uri? baseUri))
        {
            ShowLicenseStatus("Enter a valid license server URL.", isError: true);
            return;
        }

        if (string.IsNullOrWhiteSpace(licenseKey))
        {
            ShowLicenseStatus("Enter a license key before validating.", isError: true);
            return;
        }

        ValidateLicenseButton.IsEnabled = false;
        try
        {
            LauncherLicenseClient client = new(_httpClient);
            LicenseSessionInfo session = await client.StartSessionAsync(baseUri, licenseKey, HardwareFingerprintProvider.CreateSignal(), Environment.MachineName);
            _state.LicenseServerUrl = serverUrl;
            _state.LicenseKey = licenseKey;
            _state.LastLicenseSession = session;
            _state.RememberInputs = RememberInputsCheckBox.IsChecked == true;
            await _stateStore!.SaveAsync(_state);
            ShowLicenseStatus($"License active. Session expires at {session.ExpiresAtUtc:u}.", isError: false);
        }
        catch (LicenseValidationException ex)
        {
            _state.LastLicenseSession = null;
            ShowLicenseStatus($"License validation failed: {ex.Message}", isError: true);
        }
        catch (Exception ex)
        {
            _state.LastLicenseSession = null;
            ShowLicenseStatus($"Unexpected license validation error: {ex.Message}", isError: true);
        }
        finally
        {
            ValidateLicenseButton.IsEnabled = true;
        }
    }

    private void ShardListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ShardListBox.SelectedItem is not ShardDefinition shard)
        {
            SelectedShardNameTextBlock.Text = "No shard selected";
            SelectedShardDescriptionTextBlock.Text = string.Empty;
            SelectedShardEndpointTextBlock.Text = string.Empty;
            ShardWorkflowContentControl.Content = null;
            ShowSaveStatus(string.Empty, isError: false);
            return;
        }

        SelectedShardNameTextBlock.Text = shard.Name;
        SelectedShardDescriptionTextBlock.Text = shard.Description;
        SelectedShardEndpointTextBlock.Text = $"{shard.Host}:{shard.ServerPort}";
        ShowSaveStatus(string.Empty, isError: false);
        LoadShardWorkflow(shard);
    }

    private void LoadShardWorkflow(ShardDefinition shard)
    {
        if (_paths is null || _stateStore is null || _shardStateStore is null)
        {
            ShardWorkflowContentControl.Content = null;
            return;
        }

        try
        {
            IShardWorkflowControl workflow = _workflowRegistry.Resolve(shard.Id).Invoke();
            workflow.Initialize(new ShardWorkflowHostContext
            {
                Paths = _paths,
                StateStore = _stateStore,
                ShardStateStore = _shardStateStore,
                State = _state,
                HasValidLicenseSession = HasValidLicenseSession,
                ShouldRememberInputs = () => RememberInputsCheckBox.IsChecked == true,
                PublishPreparedState = RenderActiveStatePreview,
                ShowStatus = ShowSaveStatus
            }, shard);

            ShardWorkflowContentControl.Content = workflow;
        }
        catch (Exception ex)
        {
            ShardWorkflowContentControl.Content = null;
            ShowSaveStatus($"Unable to load shard workflow: {ex.Message}", true);
        }
    }

    private bool HasValidLicenseSession()
    {
        return _state.LastLicenseSession is not null &&
               _state.LastLicenseSession.ExpiresAtUtc > DateTimeOffset.UtcNow &&
               string.Equals(_state.LicenseKey, LicenseKeyTextBox.Text.Trim(), StringComparison.Ordinal) &&
               string.Equals(_state.LicenseServerUrl, LicenseServerUrlTextBox.Text.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private void RenderActiveStatePreview(ClientBootstrapDefinition bootstrap, string bootstrapTransport)
    {
        ClientBootstrapDefinition copy = new()
        {
            SchemaVersion = bootstrap.SchemaVersion,
            CreatedAtUtc = bootstrap.CreatedAtUtc,
            Shard = new ShardDefinition
            {
                Id = bootstrap.Shard.Id,
                Name = bootstrap.Shard.Name,
                Description = bootstrap.Shard.Description,
                Host = bootstrap.Shard.Host,
                Account = Mask(bootstrap.Shard.Account),
                Password = Mask(bootstrap.Shard.Password),
                UOClientVersion = bootstrap.Shard.UOClientVersion,
                ServerIP = bootstrap.Shard.ServerIP,
                ServerPort = bootstrap.Shard.ServerPort,
                Metadata = MaskMetadata(bootstrap.Shard.Metadata)
            }
        };

        ActiveStatePathTextBlock.Text = bootstrapTransport;
        ActiveStatePreviewTextBox.Text = JsonSerializer.Serialize(copy, _previewOptions);
    }

    private static string Mask(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.Length <= 8 ? new string('*', value.Length) : $"{value[..4]}...{value[^4..]}";
    }

    private static Dictionary<string, string> MaskMetadata(IReadOnlyDictionary<string, string> metadata)
    {
        Dictionary<string, string> masked = new(StringComparer.OrdinalIgnoreCase);
        foreach ((string key, string value) in metadata)
        {
            masked[key] = ShouldMask(key) ? Mask(value) : value;
        }

        return masked;
    }

    private static bool ShouldMask(string key)
    {
        string normalized = key.ToLowerInvariant();
        return normalized.Contains("password", StringComparison.Ordinal) ||
               normalized.Contains("token", StringComparison.Ordinal) ||
               normalized.Contains("secret", StringComparison.Ordinal) ||
               normalized.Contains("jwt", StringComparison.Ordinal) ||
               normalized.Contains("code", StringComparison.Ordinal) ||
               normalized.Contains("session", StringComparison.Ordinal);
    }

    private void LicenseInputsChanged(object sender, TextChangedEventArgs e)
    {
        if (_isInitializing)
        {
            return;
        }

        if (ReferenceEquals(sender, LicenseKeyTextBox) && string.Equals(_state.LicenseKey, LicenseKeyTextBox.Text, StringComparison.Ordinal))
        {
            return;
        }

        if (ReferenceEquals(sender, LicenseServerUrlTextBox) && string.Equals(_state.LicenseServerUrl, LicenseServerUrlTextBox.Text, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _state.LastLicenseSession = null;
        ShowLicenseStatus("License inputs changed. Revalidate before saving active shard state.", isError: false);
    }

    private void ShowLicenseStatus(string message, bool isError)
    {
        LicenseStatusTextBlock.Text = message;
        LicenseStatusTextBlock.Foreground = isError ? Brushes.Firebrick : Brushes.DarkGreen;
    }

    private void ShowSaveStatus(string message, bool isError)
    {
        SaveStatusTextBlock.Text = message;
        SaveStatusTextBlock.Foreground = isError ? Brushes.Firebrick : Brushes.DarkGreen;
    }
}
