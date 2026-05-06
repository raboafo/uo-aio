using System;

namespace UOAIO;

public sealed class InputQueue : IConsolidator
{
	private int _head;

	private int _tail;

	private int _size;

	private byte[] _buffer;

	public int Length => this._size;

	public InputQueue()
	{
		this._buffer = new byte[2048];
	}

	public void Clear()
	{
		this._head = 0;
		this._tail = 0;
		this._size = 0;
	}

	private void SetCapacity(int capacity)
	{
		byte[] array = new byte[capacity];
		if (this._size > 0)
		{
			if (this._head < this._tail)
			{
				Buffer.BlockCopy(this._buffer, this._head, array, 0, this._size);
			}
			else
			{
				Buffer.BlockCopy(this._buffer, this._head, array, 0, this._buffer.Length - this._head);
				Buffer.BlockCopy(this._buffer, 0, array, this._buffer.Length - this._head, this._tail);
			}
		}
		this._head = 0;
		this._tail = this._size;
		this._buffer = array;
	}

	public int GetPacketId()
	{
		if (this._size >= 1)
		{
			return this._buffer[this._head];
		}
		return -1;
	}

	public int GetPacketLength()
	{
		if (this._size >= 3)
		{
			return (this._buffer[(this._head + 1) % this._buffer.Length] << 8) | this._buffer[(this._head + 2) % this._buffer.Length];
		}
		return -1;
	}

	public ArraySegment<byte> Dequeue(int size)
	{
		if (size > this._size)
		{
			size = this._size;
		}
		ArraySegment<byte> result;
		if (size > 0)
		{
			if (this._head < this._tail)
			{
				result = new ArraySegment<byte>(this._buffer, this._head, size);
			}
			else
			{
				int num = this._buffer.Length - this._head;
				if (num >= size)
				{
					result = new ArraySegment<byte>(this._buffer, this._head, size);
				}
				else
				{
					byte[] array = new byte[size];
					Buffer.BlockCopy(this._buffer, this._head, array, 0, num);
					Buffer.BlockCopy(this._buffer, 0, array, num, size - num);
					result = new ArraySegment<byte>(array, 0, array.Length);
				}
			}
			this._head = (this._head + size) % this._buffer.Length;
			this._size -= size;
			if (this._size == 0)
			{
				this._head = 0;
				this._tail = 0;
			}
		}
		else
		{
			result = new ArraySegment<byte>(this._buffer, 0, 0);
		}
		return result;
	}

	public void Enqueue(byte[] buffer, int offset, int size)
	{
		if (this._size + size > this._buffer.Length)
		{
			this.SetCapacity((this._size + size + 2047) & -2048);
		}
		if (this._head < this._tail)
		{
			int num = this._buffer.Length - this._tail;
			if (num >= size)
			{
				Buffer.BlockCopy(buffer, offset, this._buffer, this._tail, size);
			}
			else
			{
				Buffer.BlockCopy(buffer, offset, this._buffer, this._tail, num);
				Buffer.BlockCopy(buffer, offset + num, this._buffer, 0, size - num);
			}
		}
		else
		{
			Buffer.BlockCopy(buffer, offset, this._buffer, this._tail, size);
		}
		this._tail = (this._tail + size) % this._buffer.Length;
		this._size += size;
	}
}
