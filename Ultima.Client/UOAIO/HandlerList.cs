using System;

namespace UOAIO;

internal sealed class HandlerList<TEvent> where TEvent : IEvent
{
	private Action<TEvent>[] _handlers = new Action<TEvent>[0];

	private readonly object _lock = new object();

	public void Add(Action<TEvent> handler)
	{
		lock (this._lock)
		{
			Action<TEvent>[] handlers = this._handlers;
			Action<TEvent>[] array = new Action<TEvent>[handlers.Length + 1];
			Array.Copy(handlers, array, handlers.Length);
			array[handlers.Length] = handler;
			this._handlers = array;
		}
	}

	public void Remove(Action<TEvent> handler)
	{
		lock (this._lock)
		{
			Action<TEvent>[] handlers = this._handlers;
			int num = Array.IndexOf(handlers, handler);
			if (num >= 0)
			{
				Action<TEvent>[] array = new Action<TEvent>[handlers.Length - 1];
				if (num > 0)
				{
					Array.Copy(handlers, 0, array, 0, num);
				}
				if (num < handlers.Length - 1)
				{
					Array.Copy(handlers, num + 1, array, num, handlers.Length - num - 1);
				}
				this._handlers = array;
			}
		}
	}

	public void Invoke(TEvent @event)
	{
		Action<TEvent>[] handlers = this._handlers;
		for (int i = 0; i < handlers.Length; i++)
		{
			try
			{
				handlers[i](@event);
			}
			catch (Exception ex)
			{
				Debug.Error(ex);
			}
		}
	}
}
