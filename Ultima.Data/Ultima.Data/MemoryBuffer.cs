using System;
using System.IO;

namespace Ultima.Data;

internal struct MemoryBuffer
{
	private unsafe readonly byte* left;

	private unsafe readonly byte* right;

	public unsafe byte* Pointer => this.left;

	public unsafe long Length => this.right - this.left;

	public unsafe MemoryBuffer(byte* pointer, long length)
		: this(pointer, pointer + length)
	{
	}

	public unsafe MemoryBuffer(byte* left, byte* right)
	{
		if (left == null)
		{
			throw new ArgumentNullException("left");
		}
		if (right == null)
		{
			throw new ArgumentNullException("right");
		}
		if (right < left)
		{
			throw new ArgumentOutOfRangeException("right", "Argument 'right' must be greater than or equal to argument 'left'.");
		}
		this.left = left;
		this.right = right;
	}

	public unsafe MemoryBuffer Slice(long offset, long length)
	{
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset", offset, "Argument 'offset' must be greater than or equal to zero.");
		}
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", length, "Argument 'length' must be greater than or equal to zero.");
		}
		if (offset > this.Length - length)
		{
			throw new ArgumentOutOfRangeException("offset", offset, "Offset and length must refer to a location within the buffer.");
		}
		return new MemoryBuffer(this.left + offset, this.left + offset + length);
	}

	public unsafe UnmanagedMemoryStream CreateStream()
	{
		return new UnmanagedMemoryStream(this.Pointer, this.Length);
	}
}
