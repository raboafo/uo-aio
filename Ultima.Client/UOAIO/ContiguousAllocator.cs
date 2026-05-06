using System;
using System.Collections.Generic;

namespace UOAIO;

public sealed class ContiguousAllocator : IBufferPolicy
{
	private object _syncRoot;

	private int _bufferSize;

	private Stack<byte[]> _stack;

	private int _capacity;

	private int _misses;

	public int BufferSize => this._bufferSize;

	public int Available => this._stack.Count;

	public int Capacity => this._capacity;

	public int Misses => this._misses;

	public ContiguousAllocator(int bufferSize, int initialCapacity)
	{
		if (bufferSize <= 0)
		{
			throw new ArgumentOutOfRangeException("bufferSize", bufferSize, "Buffer size must be greater than zero.");
		}
		if (initialCapacity <= 0)
		{
			throw new ArgumentOutOfRangeException("initialCapacity", initialCapacity, "Initial capacity must be greater than zero.");
		}
		this._syncRoot = new object();
		this._bufferSize = bufferSize;
		this._stack = new Stack<byte[]>(initialCapacity);
		this.EnsureCapacity(initialCapacity);
	}

	private void EnsureCapacity(int capacity)
	{
		while (this._capacity < capacity)
		{
			this._stack.Push(new byte[this._bufferSize]);
			this._capacity++;
		}
	}

	public byte[] Acquire()
	{
		lock (this._syncRoot)
		{
			if (this._stack.Count == 0)
			{
				this._misses++;
				this.EnsureCapacity(this._capacity * 2);
			}
			return this._stack.Pop();
		}
	}

	public void Release(byte[] buffer)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		lock (this._syncRoot)
		{
			this._stack.Push(buffer);
		}
	}
}
