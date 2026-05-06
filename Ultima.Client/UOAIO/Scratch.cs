using System;
using System.Collections.Generic;

namespace UOAIO;

public class Scratch<T> : IDisposable where T : new()
{
	private enum State
	{
		Empty,
		Acquired,
		Released
	}

	[ThreadStatic]
	private static Queue<T> queue;

	private T value;

	private State state;

	public T Value
	{
		get
		{
			if (this.state == State.Empty)
			{
				this.state = State.Acquired;
				this.value = this.Acquire();
			}
			else if (this.state == State.Released)
			{
				throw new ObjectDisposedException("this");
			}
			return this.value;
		}
	}

	protected virtual T Acquire()
	{
		if (Scratch<T>.queue == null)
		{
			Scratch<T>.queue = new Queue<T>();
		}
		if (Scratch<T>.queue.Count > 0)
		{
			return Scratch<T>.queue.Dequeue();
		}
		return new T();
	}

	protected virtual void Release(T value)
	{
		if (Scratch<T>.queue == null)
		{
			Scratch<T>.queue = new Queue<T>();
		}
		Scratch<T>.queue.Enqueue(value);
	}

	void IDisposable.Dispose()
	{
		if (this.state == State.Acquired)
		{
			this.state = State.Released;
			this.Release(this.value);
		}
	}
}
