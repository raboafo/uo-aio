using UOAIO.ShardRuntime;

namespace UOAIO.Launcher.ShardWorkflows;

public interface IShardWorkflowControl
{
    void Initialize(ShardWorkflowHostContext hostContext, ShardDefinition shard);
}
