using System.IO;
using System.Text;

namespace UOAIO;

public class PacketWriter
{
	private MemoryStream m_Stream;

	private int m_BufferLength;

	private byte[] m_Buffer;

	private int m_Index;

	public long Length
	{
		get
		{
			if (this.m_Index > 0)
			{
				this.Flush();
			}
			return this.m_Stream.Length;
		}
	}

	public PacketWriter()
		: this(32)
	{
	}

	public PacketWriter(int capacity)
	{
		this.m_Stream = new MemoryStream(capacity);
		this.m_BufferLength = ((capacity > 16) ? 16 : capacity);
		this.m_Buffer = new byte[this.m_BufferLength];
	}

	public void Write(string toWrite, int cb)
	{
		if (this.m_Index > 0)
		{
			this.Flush();
		}
		int num = toWrite.Length;
		if (num > cb)
		{
			num = cb;
		}
		if (num > 0)
		{
			this.Write(Encoding.ASCII.GetBytes(toWrite.ToCharArray(), 0, num));
		}
		int num2 = cb - num;
		if (num2 > 0)
		{
			this.m_Stream.Write(new byte[num2], 0, num2);
		}
	}

	public void Seek(long offset, SeekOrigin origin)
	{
		if (this.m_Index > 0)
		{
			this.Flush();
		}
		this.m_Stream.Seek(offset, origin);
	}

	public void Write(bool toWrite)
	{
		if (this.m_Index + 1 > this.m_BufferLength)
		{
			this.Flush();
		}
		this.m_Buffer[this.m_Index++] = (byte)(toWrite ? 1u : 0u);
	}

	public void Write(string toWrite)
	{
		if (this.m_Index > 0)
		{
			this.Flush();
		}
		this.Write(Encoding.ASCII.GetBytes(toWrite));
	}

	public void WriteUnicode(string toWrite)
	{
		if (this.m_Index > 0)
		{
			this.Flush();
		}
		this.Write(Encoding.BigEndianUnicode.GetBytes(toWrite));
	}

	public void WriteUnicodeLE(string toWrite)
	{
		if (this.m_Index > 0)
		{
			this.Flush();
		}
		this.Write(Encoding.Unicode.GetBytes(toWrite));
	}

	public void Write(byte[] toWrite)
	{
		if (this.m_Index > 0)
		{
			this.Flush();
		}
		this.m_Stream.Write(toWrite, 0, toWrite.Length);
	}

	public void Write(byte[] toWrite, int offset, int size)
	{
		if (this.m_Index > 0)
		{
			this.Flush();
		}
		this.m_Stream.Write(toWrite, offset, size);
	}

	public void Write(uint toWrite)
	{
		if (this.m_Index + 4 > this.m_BufferLength)
		{
			this.Flush();
		}
		this.m_Buffer[this.m_Index++] = (byte)(toWrite >> 24);
		this.m_Buffer[this.m_Index++] = (byte)(toWrite >> 16);
		this.m_Buffer[this.m_Index++] = (byte)(toWrite >> 8);
		this.m_Buffer[this.m_Index++] = (byte)toWrite;
	}

	public void WriteEncoded(int toWrite)
	{
		this.Write((byte)0);
		this.Write(toWrite);
	}

	public void Write(int toWrite)
	{
		if (this.m_Index + 4 > this.m_BufferLength)
		{
			this.Flush();
		}
		this.m_Buffer[this.m_Index++] = (byte)(toWrite >> 24);
		this.m_Buffer[this.m_Index++] = (byte)(toWrite >> 16);
		this.m_Buffer[this.m_Index++] = (byte)(toWrite >> 8);
		this.m_Buffer[this.m_Index++] = (byte)toWrite;
	}

	public void Write(ushort toWrite)
	{
		if (this.m_Index + 2 > this.m_BufferLength)
		{
			this.Flush();
		}
		this.m_Buffer[this.m_Index++] = (byte)(toWrite >> 8);
		this.m_Buffer[this.m_Index++] = (byte)toWrite;
	}

	public void Write(short toWrite)
	{
		if (this.m_Index + 2 > this.m_BufferLength)
		{
			this.Flush();
		}
		this.m_Buffer[this.m_Index++] = (byte)(toWrite >> 8);
		this.m_Buffer[this.m_Index++] = (byte)toWrite;
	}

	public void Write(byte toWrite)
	{
		if (this.m_Index + 1 > this.m_BufferLength)
		{
			this.Flush();
		}
		this.m_Buffer[this.m_Index++] = toWrite;
	}

	public void Write(sbyte toWrite)
	{
		if (this.m_Index + 1 > this.m_BufferLength)
		{
			this.Flush();
		}
		this.m_Buffer[this.m_Index++] = (byte)toWrite;
	}

	public void Close()
	{
		this.m_Stream.Close();
		this.m_Buffer = null;
	}

	public void Flush()
	{
		if (this.m_Index > 0)
		{
			this.m_Stream.Write(this.m_Buffer, 0, this.m_Index);
			this.m_Index = 0;
		}
	}

	public byte[] Compile()
	{
		this.Flush();
		return this.m_Stream.ToArray();
	}
}
