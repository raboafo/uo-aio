using System;
using System.Collections.Generic;

namespace UOAIO;

public sealed class OutputQueue : IConsolidator
{
	public sealed class Gram
	{
		private static Stack<Gram> _pool;

		private IBufferPolicy _bufferPolicy;

		private byte[] _buffer;

		private int _length;

		public byte[] Buffer => this._buffer;

		public int Length => this._length;

		public int Available => this._buffer.Length - this._length;

		public bool IsFull => this._length == this._buffer.Length;

		public static Gram Acquire(IBufferPolicy bufferPolicy)
		{
			lock (Gram._pool)
			{
				Gram gram = ((Gram._pool.Count <= 0) ? new Gram() : Gram._pool.Pop());
				gram._bufferPolicy = bufferPolicy;
				gram._buffer = bufferPolicy.Acquire();
				gram._length = 0;
				return gram;
			}
		}

		private Gram()
		{
		}

		public int Write(byte[] buffer, int offset, int length)
		{
			int num = Math.Min(length, this.Available);
			System.Buffer.BlockCopy(buffer, offset, this._buffer, this._length, num);
			this._length += num;
			return num;
		}

		public void Release()
		{
			lock (Gram._pool)
			{
				Gram._pool.Push(this);
				this._bufferPolicy.Release(this._buffer);
			}
		}

		static Gram()
		{
			Gram._pool = new Stack<Gram>();
		}
	}

	private object _syncRoot;

	private IBufferPolicy _bufferPolicy;

	private bool _isWorking;

	private Queue<Gram> _pending;

	private Gram _buffered;

	public bool IsFlushReady => this._pending.Count == 0 && this._buffered != null;

	public bool IsEmpty => this._pending.Count == 0 && this._buffered == null;

	public OutputQueue(IBufferPolicy bufferPolicy)
	{
		if (bufferPolicy == null)
		{
			throw new ArgumentNullException("bufferPolicy");
		}
		this._syncRoot = new object();
		this._bufferPolicy = bufferPolicy;
		this._pending = new Queue<Gram>();
	}

	public Gram Flush()
	{
		lock (this._syncRoot)
		{
			Gram gram = null;
			if (this._buffered != null)
			{
				if (this._pending.Count == 0)
				{
					gram = this._buffered;
				}
				this._pending.Enqueue(this._buffered);
				this._buffered = null;
			}
			this._isWorking = gram != null;
			return gram;
		}
	}

	public Gram Proceed()
	{
		lock (this._syncRoot)
		{
			Gram result = null;
			if (this._pending.Count > 0)
			{
				this._pending.Dequeue().Release();
				if (this._pending.Count > 0)
				{
					result = this._pending.Peek();
				}
				else
				{
					this._isWorking = false;
				}
			}
			else
			{
				this._isWorking = false;
			}
			return result;
		}
	}

	public Gram Query()
	{
		lock (this._syncRoot)
		{
			Gram result = null;
			if (this._pending.Count > 0 && !this._isWorking)
			{
				result = this._pending.Peek();
				this._isWorking = true;
			}
			return result;
		}
	}

	public void Enqueue(byte[] buffer, int offset, int length)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0 || offset >= buffer.Length)
		{
			throw new ArgumentOutOfRangeException("offset", offset, "Offset must be greater than or equal to zero and less than the size of the buffer.");
		}
		if (length < 0 || length > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("length", length, "Length cannot be less than zero or greater than the size of the buffer.");
		}
		if (buffer.Length - offset < length)
		{
			throw new ArgumentException("Offset and length do not point to a valid segment within the buffer.");
		}
		lock (this._syncRoot)
		{
			while (length > 0)
			{
				if (this._buffered == null)
				{
					this._buffered = Gram.Acquire(this._bufferPolicy);
				}
				int num = this._buffered.Write(buffer, offset, length);
				offset += num;
				length -= num;
				if (this._buffered.IsFull)
				{
					this._pending.Enqueue(this._buffered);
					this._buffered = null;
				}
			}
		}
	}

	public void Clear()
	{
		lock (this._syncRoot)
		{
			if (this._buffered != null)
			{
				this._buffered.Release();
				this._buffered = null;
			}
			while (this._pending.Count > 0)
			{
				this._pending.Dequeue().Release();
			}
			this._isWorking = false;
		}
	}
}
