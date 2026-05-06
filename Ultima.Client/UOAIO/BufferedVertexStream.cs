using SharpDX;
using SharpDX.Direct3D9;

namespace UOAIO;

public class BufferedVertexStream
{
	private int m_VertexBufferOffset;

	private int m_VertexBufferLength;

	private int m_SizePerVertex;

	private VertexBuffer m_Buffer;

	private DataStream m_Stream;

	public int Length => this.m_VertexBufferLength;

	public BufferedVertexStream(VertexBuffer buffer, int vertexBufferLength, int sizePerVertex)
	{
		this.m_Buffer = buffer;
		this.m_VertexBufferLength = vertexBufferLength;
		this.m_SizePerVertex = sizePerVertex;
	}

	public void Unlock()
	{
		if (this.m_Stream != null)
		{
			try
			{
				this.m_Stream.Close();
				this.m_Buffer.Unlock();
			}
			catch
			{
			}
			this.m_Stream = null;
		}
	}

	public int Push(byte[] buffer, int vertexOffset, int vertexCount, bool unlock)
	{
		int result;
		if (this.m_VertexBufferLength >= this.m_VertexBufferOffset + vertexCount)
		{
			if (this.m_Stream == null)
			{
				if (unlock)
				{
					this.m_Stream = this.m_Buffer.Lock(this.m_VertexBufferOffset * this.m_SizePerVertex, vertexCount * this.m_SizePerVertex, SharpDX.Direct3D9.LockFlags.NoOverwrite);
				}
				else
				{
					this.m_Stream = this.m_Buffer.Lock(this.m_VertexBufferOffset * this.m_SizePerVertex, (this.m_VertexBufferLength - this.m_VertexBufferOffset) * this.m_SizePerVertex, SharpDX.Direct3D9.LockFlags.NoOverwrite);
				}
			}
			this.m_Stream.WriteRange(buffer, vertexOffset * this.m_SizePerVertex, vertexCount * this.m_SizePerVertex);
			result = this.m_VertexBufferOffset;
			this.m_VertexBufferOffset += vertexCount;
			if (unlock)
			{
				this.Unlock();
			}
		}
		else if (vertexCount <= this.m_VertexBufferLength)
		{
			this.Unlock();
			if (this.m_Stream == null)
			{
				if (unlock)
				{
					this.m_Stream = this.m_Buffer.Lock(0, vertexCount * this.m_SizePerVertex, SharpDX.Direct3D9.LockFlags.Discard);
				}
				else
				{
					this.m_Stream = this.m_Buffer.Lock(0, this.m_VertexBufferLength * this.m_SizePerVertex, SharpDX.Direct3D9.LockFlags.Discard);
				}
			}
			this.m_Stream.WriteRange(buffer, vertexOffset * this.m_SizePerVertex, vertexCount * this.m_SizePerVertex);
			result = 0;
			this.m_VertexBufferOffset = vertexCount;
			if (unlock)
			{
				this.Unlock();
			}
		}
		else
		{
			result = -1;
		}
		return result;
	}
}
