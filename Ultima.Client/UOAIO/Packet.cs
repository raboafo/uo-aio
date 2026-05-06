using System.IO;

namespace UOAIO;

public class Packet
{
	protected int m_Length;

	protected bool m_Encode = true;

	public PacketWriter m_Stream;

	public bool Encode => this.m_Encode;

	public Packet(byte packetID)
	{
		this.m_Stream = new PacketWriter();
		this.m_Stream.Write(packetID);
		this.m_Stream.Write((short)0);
	}

	public Packet(byte packetID, int length)
	{
		this.m_Length = length;
		this.m_Stream = new PacketWriter(length);
		this.m_Stream.Write(packetID);
	}

	public void Dispose()
	{
		this.m_Stream.Close();
		this.m_Stream = null;
	}

	public byte[] Compile()
	{
		this.m_Stream.Flush();
		if (this.m_Length == 0)
		{
			long length = this.m_Stream.Length;
			this.m_Stream.Seek(1L, SeekOrigin.Begin);
			this.m_Stream.Write((ushort)length);
			this.m_Stream.Flush();
		}
		return this.m_Stream.Compile();
	}
}
