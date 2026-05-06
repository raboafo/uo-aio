using System;
using System.Collections.Concurrent;

namespace UOAIO;

public sealed class EventBus
{
	private readonly ConcurrentDictionary<Type, object> _handlers = new ConcurrentDictionary<Type, object>();

	public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IEvent
	{
		this.GetOrCreate<TEvent>().Add(handler);
	}

	public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : IEvent
	{
		if (this._handlers.TryGetValue(typeof(TEvent), out var value))
		{
			((HandlerList<TEvent>)value).Remove(handler);
		}
	}

	public void Publish<TEvent>(TEvent @event) where TEvent : IEvent
	{
		if (this._handlers.TryGetValue(typeof(TEvent), out var value))
		{
			((HandlerList<TEvent>)value).Invoke(@event);
		}
	}

	private HandlerList<TEvent> GetOrCreate<TEvent>() where TEvent : IEvent
	{
		return (HandlerList<TEvent>)this._handlers.GetOrAdd(typeof(TEvent), (Type _) => new HandlerList<TEvent>());
	}
}
