using System;

namespace UOAIO;

public sealed class BufferAllocator : IBufferPolicy
{
	private readonly int bufferSize;

	public int BufferSize => this.bufferSize;

	public BufferAllocator(int bufferSize)
	{
		if (bufferSize <= 0)
		{
			throw new ArgumentOutOfRangeException("bufferSize", bufferSize, "Argument 'bufferSize' must be greater than zero.");
		}
		this.bufferSize = bufferSize;
	}

	public byte[] Acquire()
	{
		return new byte[this.bufferSize];
	}

	public void Release(byte[] buffer)
	{
	}
}
