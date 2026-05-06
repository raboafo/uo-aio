using System;
using UOAIO.ShardRuntime;

namespace UOAIO;

internal sealed class ClassicClientShardHandler : IClientBootstrapHandler
{
    private ShardDefinition _shard;

    public string ShardId => "new-renaissance";

    public void InitializeBootstrap(ClientBootstrapDefinition bootstrap)
    {
        if (bootstrap == null || bootstrap.Shard == null)
        {
            throw new ArgumentNullException("bootstrap");
        }

        _shard = bootstrap.Shard;
    }

    public void SendFirstLogin()
    {
        Network.Send(new PAccount(_shard.Account, _shard.Password));
    }
}
