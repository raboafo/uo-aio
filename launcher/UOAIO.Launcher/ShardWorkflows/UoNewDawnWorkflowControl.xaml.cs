using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UOAIO.Launcher.Core;
using UOAIO.ShardRuntime;
using Brushes = System.Windows.Media.Brushes;
using UserControl = System.Windows.Controls.UserControl;

namespace UOAIO.Launcher.ShardWorkflows;

public partial class UoNewDawnWorkflowControl : UserControl, IShardWorkflowControl
{
    private readonly UoNewDawnAuthorizationService _authorizationService = new();
    private readonly UoNewDawnWorkflowState _workflowState = new();

    private ShardWorkflowHostContext? _hostContext;
    private ShardDefinition? _shard;

    public UoNewDawnWorkflowControl()
    {
        InitializeComponent();
    }

    public void Initialize(ShardWorkflowHostContext hostContext, ShardDefinition shard)
    {
        _hostContext = hostContext;
        _shard = hostContext.ShardStateStore.ApplyRememberedState(shard);

        _workflowState.RefreshToken = _shard.Metadata.TryGetValue("refresh_token", out string? refreshToken) &&
                                      bool.TryParse(refreshToken, out bool parsedRefreshToken) &&
                                      parsedRefreshToken;
        RefreshTokenCheckBox.IsChecked = _workflowState.RefreshToken;
        _workflowState.RestoreSelectedAccount(_shard.Account);
        AssetDirectoryTextBox.Text = ShardWorkflowAssetPathSupport.GetAssetPath(_shard);

        ShowAuthorizationStatus("Discord authorization is required before continuing.", isError: false);
        ShowAccountStatus("Choose the account that should be handed off to the client.", isError: false);
        RenderStage();
    }

    private async void AuthorizeButton_Click(object sender, RoutedEventArgs e)
    {
        if (_shard is null)
        {
            return;
        }

        AuthorizeButton.IsEnabled = false;
        ContinueButton.IsEnabled = false;
        ShowAuthorizationStatus("Starting Discord authorization...", isError: false);
        try
        {
            UoNewDawnAuthorizationResult authorization = await _authorizationService
                .AcquireAuthorizationAsync(_shard, _workflowState.RefreshToken)
                .ConfigureAwait(true);
            _workflowState.ApplyAuthorization(authorization);
            ShowAuthorizationStatus($"Authorized as {authorization.IdentityHint}.", isError: false);
            ShowAccountStatus("Choose the account that should be handed off to the client.", isError: false);
            RenderStage();
        }
        catch (Exception ex)
        {
            ShowAuthorizationStatus($"Authorization failed: {ex.Message}", isError: true);
            RenderStage();
        }
        finally
        {
            AuthorizeButton.IsEnabled = true;
        }
    }

    private void ContinueButton_Click(object sender, RoutedEventArgs e)
    {
        _workflowState.GoForward();
        RenderStage();
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        _workflowState.GoBack();
        RenderStage();
    }

    private async void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        if (_hostContext is null || _shard is null || _workflowState.Authorization is null)
        {
            return;
        }

        PlayButton.IsEnabled = false;
        try
        {
            if (!ShardWorkflowAssetPathSupport.TryNormalizeAssetPath(AssetDirectoryTextBox.Text, out string assetPath, out string? assetPathError))
            {
                ShowAccountStatus(assetPathError ?? "Asset directory is invalid.", isError: true);
                _hostContext.ShowStatus(assetPathError ?? "Asset directory is invalid.", true);
                return;
            }

            Dictionary<string, string> metadata = _workflowState.BuildRuntimeMetadata(AccountComboBox.SelectedItem as string ?? string.Empty);
            metadata["refresh_token"] = _workflowState.RefreshToken.ToString();
            string loginHost = _workflowState.Authorization.LoginHost;
            int loginPort = _workflowState.Authorization.LoginPort;
            System.Net.IPAddress resolvedIpAddress = await ShardConnectionResolver.ResolveIpAddressAsync(loginHost).ConfigureAwait(true);

            ShardDefinition runtimeShard = new()
            {
                Id = _shard.Id,
                Name = _shard.Name,
                Description = _shard.Description,
                Host = loginHost,
                Account = metadata["account"],
                Password = metadata["password"],
                ClientVersion = _shard.ClientVersion,
                ServerIP = resolvedIpAddress,
                ServerPort = loginPort,
                Metadata = new Dictionary<string, string>(_shard.Metadata, StringComparer.OrdinalIgnoreCase)
            };
            foreach ((string key, string value) in metadata)
            {
                runtimeShard.Metadata[key] = value;
            }
            ShardWorkflowAssetPathSupport.ApplyAssetPathMetadata(runtimeShard.Metadata, assetPath);

            ClientBootstrapDefinition bootstrap = ClientBootstrapDefinitionFactory.Create(runtimeShard);
            ClientProcessLauncher launcher = new();
            await launcher.StartWithBootstrapAsync(AppContext.BaseDirectory, bootstrap).ConfigureAwait(true);

            _hostContext.State.SelectedShardId = _shard.Id;
            _hostContext.State.RememberInputs = _hostContext.ShouldRememberInputs();
            await _hostContext.StateStore.SaveAsync(_hostContext.State).ConfigureAwait(true);
            if (_hostContext.State.RememberInputs)
            {
                await _hostContext.ShardStateStore.SaveAsync(runtimeShard).ConfigureAwait(true);
            }
            else
            {
                await _hostContext.ShardStateStore.DeleteAsync(_shard.Id).ConfigureAwait(true);
            }

            _hostContext.PublishPreparedState(bootstrap, "named-pipe://ephemeral");
            ShowAccountStatus($"Launched client for {_workflowState.SelectedAccount}.", isError: false);
            _hostContext.ShowStatus("Launched client over a named-pipe bootstrap transport.", false);
        }
        catch (Exception ex)
        {
            ShowAccountStatus($"Unable to launch New Dawn client: {ex.Message}", isError: true);
            _hostContext.ShowStatus($"Unable to launch New Dawn client: {ex.Message}", true);
        }
        finally
        {
            PlayButton.IsEnabled = true;
        }
    }

    private void RefreshTokenCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        _workflowState.RefreshToken = RefreshTokenCheckBox.IsChecked == true;
    }

    private void AccountComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _workflowState.SetSelectedAccount(AccountComboBox.SelectedItem as string);
    }

    private void RenderStage()
    {
        bool isAuthorizeStage = _workflowState.CurrentStage == UoNewDawnWorkflowStage.Authorize;
        AuthorizeStagePanel.Visibility = isAuthorizeStage ? Visibility.Visible : Visibility.Collapsed;
        AccountStagePanel.Visibility = isAuthorizeStage ? Visibility.Collapsed : Visibility.Visible;

        if (isAuthorizeStage)
        {
            StageTitleTextBlock.Text = "Step 1 of 2 - Discord authorization";
            StageDescriptionTextBlock.Text = "Authorize the launcher with Discord. If you go back here later, you can continue with the existing session or refresh it.";
        }
        else
        {
            StageTitleTextBlock.Text = "Step 2 of 2 - Account selection";
            StageDescriptionTextBlock.Text = "Choose the account that should be used when the client takes over. If authorization changes, this list will be rebuilt from the new login state.";
        }

        BackButton.IsEnabled = _workflowState.CanGoBack;
        ContinueButton.IsEnabled = _workflowState.CanGoForward;
        ContinueButton.Visibility = isAuthorizeStage ? Visibility.Visible : Visibility.Collapsed;

        AuthorizedIdentityTextBlock.Text = _workflowState.Authorization is null
            ? "Authorized user"
            : $"Welcome @{_workflowState.Authorization.IdentityHint}";

        AccountComboBox.ItemsSource = null;
        AccountComboBox.ItemsSource = _workflowState.AccountOptions;
        if (_workflowState.AccountOptions.Count > 0)
        {
            AccountComboBox.SelectedItem = _workflowState.SelectedAccount;
        }
    }

    private void ShowAuthorizationStatus(string message, bool isError)
    {
        AuthorizationStatusTextBlock.Text = message;
        AuthorizationStatusTextBlock.Foreground = isError ? Brushes.Firebrick : Brushes.DarkGreen;
    }

    private void ShowAccountStatus(string message, bool isError)
    {
        AccountStageStatusTextBlock.Text = message;
        AccountStageStatusTextBlock.Foreground = isError ? Brushes.Firebrick : Brushes.DarkGreen;
    }

    private void BrowseAssetDirectoryButton_Click(object sender, RoutedEventArgs e)
    {
        string? selectedPath = ShardWorkflowAssetPathSupport.BrowseForAssetPath(AssetDirectoryTextBox.Text);
        if (!string.IsNullOrWhiteSpace(selectedPath))
        {
            AssetDirectoryTextBox.Text = selectedPath;
        }
    }

    private void ClearAssetDirectoryButton_Click(object sender, RoutedEventArgs e)
    {
        AssetDirectoryTextBox.Text = string.Empty;
    }

    private static Version? ResolveClientVersion(IReadOnlyDictionary<string, string> metadata, Version? fallback)
    {
        if (metadata.TryGetValue("client_version", out string? value) &&
            !string.IsNullOrWhiteSpace(value) &&
            Version.TryParse(value, out Version? parsed))
        {
            return parsed;
        }

        return fallback;
    }
}
