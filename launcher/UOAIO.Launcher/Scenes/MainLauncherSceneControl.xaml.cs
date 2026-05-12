using System.Text.Json;
using System.Windows.Controls;
using UOAIO.Launcher.Core;
using UOAIO.Launcher.ShardWorkflows;
using UOAIO.ShardRuntime;
using Brushes = System.Windows.Media.Brushes;
using UserControl = System.Windows.Controls.UserControl;

namespace UOAIO.Launcher.Scenes;

public partial class MainLauncherSceneControl : UserControl
{
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
        new ShardWorkflowRegistration<Func<IShardWorkflowControl>>("uo-new-dawn", static () => new UoNewDawnWorkflowControl()),
        new ShardWorkflowRegistration<Func<IShardWorkflowControl>>("tides-of-power", static () => new TidesOfPowerWorkflowControl())
    });

    private bool _isInitializing;
    private LauncherPaths? _paths;
    private LauncherStateStore? _stateStore;
    private ShardDefinitionStateStore? _shardStateStore;
    private LauncherState? _state;
    private ShardCatalog _catalog = new();
    private LicenseGateState _licenseState = new();
    private bool _hasLoadedCatalog;

    public MainLauncherSceneControl()
    {
        InitializeComponent();
    }

    public async Task InitializeAsync(
        LauncherPaths paths,
        LauncherStateStore stateStore,
        ShardDefinitionStateStore shardStateStore,
        LauncherState state,
        LicenseGateState licenseState)
    {
        _paths = paths;
        _stateStore = stateStore;
        _shardStateStore = shardStateStore;
        _state = state;
        _licenseState = licenseState;

        _isInitializing = true;
        try
        {
            RememberInputsCheckBox.IsChecked = state.RememberInputs;
            if (!_hasLoadedCatalog)
            {
                ActiveStatePathTextBlock.Text = "named-pipe://not-prepared";
                ActiveStatePreviewTextBox.Text = "No client bootstrap has been prepared yet.";

                ShardCatalogService catalogService = new();
                try
                {
                    _catalog = await catalogService.LoadAsync(paths.ShardManifestPath).ConfigureAwait(true);
                    ShardListBox.ItemsSource = _catalog.Shards;
                    if (!string.IsNullOrWhiteSpace(state.SelectedShardId))
                    {
                        ShardDefinition? preferred = _catalog.Shards.FirstOrDefault(shard => string.Equals(shard.Id, state.SelectedShardId, StringComparison.OrdinalIgnoreCase));
                        if (preferred is not null)
                        {
                            ShardListBox.SelectedItem = preferred;
                        }
                    }

                    if (ShardListBox.SelectedItem is null && _catalog.Shards.Count > 0)
                    {
                        ShardListBox.SelectedIndex = 0;
                    }

                    _hasLoadedCatalog = true;
                }
                catch (Exception ex)
                {
                    ShowSaveStatus($"Failed to load shard catalog: {ex.Message}", isError: true);
                }
            }
        }
        finally
        {
            _isInitializing = false;
        }

        UpdateLicenseBanner(licenseState);
    }

    public void UpdateLicenseBanner(LicenseGateState licenseState)
    {
        _licenseState = licenseState;
        if (licenseState.Summary is null)
        {
            LicenseBannerTextBlock.Text = licenseState.StatusMessage;
            return;
        }

        string mode = licenseState.AccessMode == LicenseAccessMode.Offline ? "Offline grant" : "Online session";
        LicenseBannerTextBlock.Text = $"{mode} active for license {licenseState.Summary.LicenseId} until {licenseState.Summary.ExpiresAtUtc:u}.";
    }

    private void ShardListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing)
        {
            return;
        }

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
        if (_paths is null || _stateStore is null || _shardStateStore is null || _state is null)
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
                HasValidLicenseSession = () => _licenseState.CanEnterLauncher,
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
                ClientVersion = bootstrap.Shard.ClientVersion,
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

    private void ShowSaveStatus(string message, bool isError)
    {
        SaveStatusTextBlock.Text = message;
        SaveStatusTextBlock.Foreground = isError ? Brushes.Firebrick : Brushes.DarkGreen;
    }
}
