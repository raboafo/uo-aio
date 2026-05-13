using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
        StatusTextBlock.Foreground = ResolveStatusBrush(gateState.CanEnterLauncher ? "StatusSuccessBrush" : "StatusErrorBrush");
        EndpointTextBlock.Text = string.IsNullOrWhiteSpace(gateState.EffectiveServerUrl)
            ? string.Empty
            : $"Endpoint: {gateState.EffectiveServerUrl}";
    }

    public void SetBusy(bool isBusy)
    {
        ValidateButton.IsEnabled = !isBusy;
    }

    public void SetLicenseEntryVisible(bool isVisible)
    {
        LicenseEntryPanel.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ValidateButton_Click(object sender, RoutedEventArgs e)
    {
        ValidateRequested?.Invoke(this, new LicenseValidationRequestedEventArgs
        {
            LicenseKey = LicenseKeyTextBox.Text.Trim(),
            DeveloperServerOverride = ServerOverrideTextBox.Text.Trim()
        });
    }

    private System.Windows.Media.Brush ResolveStatusBrush(string resourceKey)
    {
        return TryFindResource(resourceKey) as System.Windows.Media.Brush ?? System.Windows.Media.Brushes.White;
    }
}
