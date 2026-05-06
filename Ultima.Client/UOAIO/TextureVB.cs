using System;

namespace UOAIO;

public class TextureVB : IVertexStorage
{
	private static readonly int VertexSize;

	public static ContiguousAllocator[] _allocators;

	public int _misses;

	public byte[] _buffer;

	public int _length;

	public int m_Count;

	public int m_Frame;

	public TextureVB()
	{
		this._buffer = TextureVB._allocators[this._misses].Acquire();
		this.m_Frame = -1;
	}

	public void Release()
	{
		if (this._misses < TextureVB._allocators.Length)
		{
			TextureVB._allocators[this._misses].Release(this._buffer);
		}
	}

	public ArraySegment<byte> Store(int vertexCount, int primitiveCount)
	{
		if (Renderer._profile != null)
		{
			Renderer._profile._storeTime.Start();
		}
		if (this.m_Frame != Renderer._renderCount)
		{
			this.m_Frame = Renderer._renderCount;
			this.m_Count = 0;
			this._length = 0;
		}
		this.m_Count += primitiveCount;
		int num = vertexCount * TextureVB.VertexSize;
		this._length += num;
		if (this._length > this._buffer.Length)
		{
			byte[] array = this._buffer;
			do
			{
				if (this._misses < TextureVB._allocators.Length)
				{
					TextureVB._allocators[this._misses].Release(array);
				}
				this._misses++;
				array = ((this._misses >= TextureVB._allocators.Length) ? new byte[array.Length * 2] : TextureVB._allocators[this._misses].Acquire());
			}
			while (this._length > array.Length);
			Buffer.BlockCopy(this._buffer, 0, array, 0, this._length - num);
			this._buffer = array;
		}
		ArraySegment<byte> result = new ArraySegment<byte>(this._buffer, this._length - num, num);
		if (Renderer._profile != null)
		{
			Renderer._profile._storeTime.Stop();
		}
		return result;
	}

	unsafe static TextureVB()
	{
		TextureVB.VertexSize = sizeof(TransformedColoredTextured);
		TextureVB._allocators = new ContiguousAllocator[7]
		{
			new ContiguousAllocator(4 * TextureVB.VertexSize, 4096),
			new ContiguousAllocator(8 * TextureVB.VertexSize, 2048),
			new ContiguousAllocator(16 * TextureVB.VertexSize, 1024),
			new ContiguousAllocator(32 * TextureVB.VertexSize, 512),
			new ContiguousAllocator(64 * TextureVB.VertexSize, 256),
			new ContiguousAllocator(128 * TextureVB.VertexSize, 128),
			new ContiguousAllocator(256 * TextureVB.VertexSize, 64)
		};
	}
}
