using System.Windows;
using System.Windows.Controls;
using UOAIO.Launcher.Core;
using UserControl = System.Windows.Controls.UserControl;

namespace UOAIO.Launcher.Scenes;

public sealed class LicenseValidationRequestedEventArgs : EventArgs
{
    public string LicenseKey { get; init; } = string.Empty;

    public string DeveloperServerOverride { get; init; } = string.Empty;
}

public partial class LicenseGateSceneControl : UserControl
{
    public event EventHandler<LicenseValidationRequestedEventArgs>? ValidateRequested;

    public LicenseGateSceneControl()
    {
        InitializeComponent();
        if (LauncherDefaults.AllowDeveloperLicenseServerOverride)
        {
            DeveloperOverridePanel.Visibility = Visibility.Visible;
        }
    }

    public void SetState(LicenseGateState gateState, LauncherState launcherState)
    {
        LicenseKeyTextBox.Text = gateState.RecoveredLicenseKey ?? string.Empty;
        ServerOverrideTextBox.Text = launcherState.DeveloperLicenseServerUrlOverride ?? string.Empty;
        StatusTextBlock.Text = gateState.StatusMessage;
        StatusTextBlock.Foreground = gateState.CanEnterLauncher
            ? System.Windows.Media.Brushes.DarkGreen
            : System.Windows.Media.Brushes.Firebrick;
        EndpointTextBlock.Text = string.IsNullOrWhiteSpace(gateState.EffectiveServerUrl)
            ? string.Empty
            : $"Endpoint: {gateState.EffectiveServerUrl}";
    }

    public void SetBusy(bool isBusy)
    {
        ValidateButton.IsEnabled = !isBusy;
    }

    private void ValidateButton_Click(object sender, RoutedEventArgs e)
    {
        ValidateRequested?.Invoke(this, new LicenseValidationRequestedEventArgs
        {
            LicenseKey = LicenseKeyTextBox.Text.Trim(),
            DeveloperServerOverride = ServerOverrideTextBox.Text.Trim()
        });
    }
}
