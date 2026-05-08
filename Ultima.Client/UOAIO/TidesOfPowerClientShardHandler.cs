using System;
using UOAIO.ShardRuntime;

namespace UOAIO;

internal sealed class TidesOfPowerClientShardHandler : IClientBootstrapHandler
{
    private ShardDefinition _shard;

    public string ShardId => "tides-of-power";

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
        Network.Send(new PLoginSeed(0x83A9170A, _shard.ClientVersion));
        Network.Send(new PAccount(_shard.Account, _shard.Password));
    }
}
