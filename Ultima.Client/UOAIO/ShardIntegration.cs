using System;
using System.Collections.Generic;
using UOAIO.ShardRuntime;

namespace UOAIO;

internal interface IClientBootstrapHandler : IClientShardHandler
{
	void InitializeBootstrap(ClientBootstrapDefinition bootstrap);
	void SendFirstLogin();
}

internal sealed class ClientShardHandlerRegistry
{
	private readonly Dictionary<string, IClientBootstrapHandler> _handlers;

	public ClientShardHandlerRegistry(IEnumerable<IClientBootstrapHandler> handlers)
	{
		this._handlers = new Dictionary<string, IClientBootstrapHandler>(StringComparer.OrdinalIgnoreCase);
		foreach (IClientBootstrapHandler handler in handlers)
		{
			this._handlers[handler.ShardId] = handler;
		}
	}

	public IClientBootstrapHandler Resolve(string shardId)
	{
		if (this._handlers.TryGetValue(shardId, out IClientBootstrapHandler handler))
		{
			return handler;
		}

		throw new InvalidOperationException("No client shard handler is registered for shard '" + shardId + "'.");
	}
}
