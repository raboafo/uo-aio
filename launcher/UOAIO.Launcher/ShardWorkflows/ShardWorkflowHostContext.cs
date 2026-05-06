using UOAIO.Launcher.Core;
using UOAIO.ShardRuntime;

namespace UOAIO.Launcher.ShardWorkflows;

public sealed class ShardWorkflowHostContext
{
    public required LauncherPaths Paths { get; init; }

    public required LauncherStateStore StateStore { get; init; }

    public required ShardDefinitionStateStore ShardStateStore { get; init; }

    public required LauncherState State { get; init; }

    public required Func<bool> HasValidLicenseSession { get; init; }

    public required Func<bool> ShouldRememberInputs { get; init; }

    public required Action<ClientBootstrapDefinition, string> PublishPreparedState { get; init; }

    public required Action<string, bool> ShowStatus { get; init; }
}
