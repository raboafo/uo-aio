using System.Windows;
using System.Windows.Controls;
using UOAIO.Launcher.Core;
using UOAIO.ShardRuntime;

namespace UOAIO.Launcher.ShardWorkflows;

public partial class NewRenaissanceWorkflowControl : UserControl, IShardWorkflowControl
{
    private ShardWorkflowHostContext? _hostContext;
    private ShardDefinition? _shard;

    public NewRenaissanceWorkflowControl()
    {
        InitializeComponent();
    }

    public void Initialize(ShardWorkflowHostContext hostContext, ShardDefinition shard)
    {
        _hostContext = hostContext;
        _shard = hostContext.ShardStateStore.ApplyRememberedState(shard);

        UsernameTextBox.Text = _shard.Account;
        PasswordInput.Password = _shard.Password;
    }

    private async void PrepareButton_Click(object sender, RoutedEventArgs e)
    {
        if (_hostContext is null || _shard is null)
        {
            return;
        }

        string username = UsernameTextBox.Text.Trim();
        string password = PasswordInput.Password;
        List<string> errors = new();
        if (string.IsNullOrWhiteSpace(username))
        {
            errors.Add("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            errors.Add("Password is required.");
        }

        if (errors.Count > 0)
        {
            _hostContext.ShowStatus(string.Join(Environment.NewLine, errors), true);
            return;
        }

        PrepareButton.IsEnabled = false;
        try
        {
            System.Net.IPAddress resolvedIpAddress = await ShardConnectionResolver.ResolveIpAddressAsync(_shard.Host).ConfigureAwait(true);
            ShardDefinition runtimeShard = new()
            {
                Id = _shard.Id,
                Name = _shard.Name,
                Description = _shard.Description,
                Host = _shard.Host,
                Account = username,
                Password = password,
                UOClientVersion = _shard.UOClientVersion,
                ServerIP = resolvedIpAddress,
                ServerPort = _shard.ServerPort,
                Metadata = new Dictionary<string, string>(_shard.Metadata, StringComparer.OrdinalIgnoreCase)
            };

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
            _hostContext.ShowStatus("Launched client over a named-pipe bootstrap transport.", false);
        }
        catch (Exception ex)
        {
            _hostContext.ShowStatus($"Unable to launch client: {ex.Message}", true);
        }
        finally
        {
            PrepareButton.IsEnabled = true;
        }
    }
}
