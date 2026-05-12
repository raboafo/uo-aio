using System.Net.Http;
using System.Windows;
using System.Windows.Threading;
using UOAIO.Launcher.Core;
using UOAIO.Launcher.Scenes;

namespace UOAIO.Launcher;

public partial class MainWindow : Window
{
    private readonly HttpClient _httpClient = new();
    private readonly DispatcherTimer _heartbeatTimer = new();

    private LauncherPaths? _paths;
    private LauncherStateStore? _stateStore;
    private ShardDefinitionStateStore? _shardStateStore;
    private ProtectedLicenseSecretStore? _secretStore;
    private LauncherLicenseService? _licenseService;
    private LauncherState _state = new();
    private LicenseGateSceneControl? _licenseGateScene;
    private MainLauncherSceneControl? _mainLauncherScene;

    public MainWindow()
    {
        InitializeComponent();
        _heartbeatTimer.Tick += HeartbeatTimer_Tick;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        _paths = LauncherPaths.CreateDefault(AppContext.BaseDirectory);
        _stateStore = new LauncherStateStore(_paths);
        _shardStateStore = new ShardDefinitionStateStore(_paths);
        _secretStore = new ProtectedLicenseSecretStore(_paths);
        _state = await _stateStore.LoadAsync();

        LicenseTrustSettings trustSettings = new()
        {
            AllowDeveloperOverride = LauncherDefaults.AllowDeveloperLicenseServerOverride
        };
        LauncherLicenseClient licenseClient = new(_httpClient, trustSettings);
        _licenseService = new LauncherLicenseService(_stateStore, _secretStore, licenseClient, trustSettings);

        _licenseGateScene = new LicenseGateSceneControl();
        _licenseGateScene.ValidateRequested += LicenseGateScene_ValidateRequested;

        LicenseGateState gateState = await _licenseService.InitializeAsync(_state).ConfigureAwait(true);
        await ShowSceneAsync(gateState).ConfigureAwait(true);
    }

    private async void LicenseGateScene_ValidateRequested(object? sender, LicenseValidationRequestedEventArgs e)
    {
        if (_licenseGateScene is null || _licenseService is null || _stateStore is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(e.LicenseKey))
        {
            _licenseGateScene.SetState(new LicenseGateState
            {
                AccessMode = LicenseAccessMode.Unlicensed,
                StatusMessage = "Enter a license key before validating.",
                RecoveredLicenseKey = string.Empty
            }, _state);
            return;
        }

        _state.DeveloperLicenseServerUrlOverride = LauncherDefaults.AllowDeveloperLicenseServerOverride
            ? e.DeveloperServerOverride
            : string.Empty;

        _licenseGateScene.SetBusy(true);
        try
        {
            await _stateStore.SaveAsync(_state).ConfigureAwait(true);
            LicenseGateState gateState = await _licenseService.ValidateAsync(_state, e.LicenseKey).ConfigureAwait(true);
            await ShowSceneAsync(gateState).ConfigureAwait(true);
        }
        catch (LicenseValidationException ex)
        {
            _licenseGateScene.SetState(new LicenseGateState
            {
                AccessMode = LicenseAccessMode.Unlicensed,
                StatusMessage = $"License validation failed: {ex.Message}",
                RecoveredLicenseKey = e.LicenseKey.Trim()
            }, _state);
        }
        catch (Exception ex)
        {
            _licenseGateScene.SetState(new LicenseGateState
            {
                AccessMode = LicenseAccessMode.Unlicensed,
                StatusMessage = $"Unexpected validation error: {ex.Message}",
                RecoveredLicenseKey = e.LicenseKey.Trim()
            }, _state);
        }
        finally
        {
            _licenseGateScene.SetBusy(false);
        }
    }

    private async Task ShowSceneAsync(LicenseGateState gateState)
    {
        if (gateState.CanEnterLauncher)
        {
            _mainLauncherScene ??= new MainLauncherSceneControl();
            if (_paths is not null && _stateStore is not null && _shardStateStore is not null)
            {
                await _mainLauncherScene.InitializeAsync(_paths, _stateStore, _shardStateStore, _state, gateState).ConfigureAwait(true);
                _mainLauncherScene.UpdateLicenseBanner(gateState);
            }

            SceneHost.Content = _mainLauncherScene;
            ConfigureHeartbeat(gateState);
            return;
        }

        _heartbeatTimer.Stop();
        _licenseGateScene ??= new LicenseGateSceneControl();
        _licenseGateScene.ValidateRequested -= LicenseGateScene_ValidateRequested;
        _licenseGateScene.ValidateRequested += LicenseGateScene_ValidateRequested;
        _licenseGateScene.SetState(gateState, _state);
        SceneHost.Content = _licenseGateScene;
    }

    private void ConfigureHeartbeat(LicenseGateState gateState)
    {
        int heartbeatMinutes = gateState.Summary?.Policy.HeartbeatMinutes ?? 15;
        if (heartbeatMinutes <= 0)
        {
            heartbeatMinutes = 15;
        }

        _heartbeatTimer.Interval = TimeSpan.FromMinutes(Math.Max(1, heartbeatMinutes));
        _heartbeatTimer.Start();
    }

    private async void HeartbeatTimer_Tick(object? sender, EventArgs e)
    {
        if (_licenseService is null)
        {
            return;
        }

        _heartbeatTimer.Stop();
        try
        {
            LicenseGateState gateState = await _licenseService.HeartbeatAsync(_state).ConfigureAwait(true);
            await ShowSceneAsync(gateState).ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            await ShowSceneAsync(new LicenseGateState
            {
                AccessMode = LicenseAccessMode.Unlicensed,
                StatusMessage = $"Heartbeat failed: {ex.Message}"
            }).ConfigureAwait(true);
        }
    }
}
