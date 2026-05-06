using System;
using System.IO;
using Veritas.Compression;

namespace UOAIO.Videos;

public class GZBlockIn : Stream
{
	private MemoryStream m_Uncomp;

	private BinaryReader m_In;

	private BinaryReader m_Self;

	private bool m_Compressed;

	private static byte[] m_ReadBuff;

	private static byte[] m_CompBuff;

	public Stream RawStream => this.m_In.BaseStream;

	public BinaryReader Raw => this.m_In;

	public BinaryReader Compressed => this.m_Compressed ? this.m_Self : this.m_In;

	public bool IsCompressed
	{
		get
		{
			return this.m_Compressed;
		}
		set
		{
			this.m_Compressed = value;
		}
	}

	public override bool CanSeek => true;

	public override bool CanRead => true;

	public override bool CanWrite => false;

	public override long Length => (!this.m_Compressed) ? this.RawStream.Length : ((this.RawStream.Position < this.RawStream.Length) ? int.MaxValue : this.m_Uncomp.Length);

	public override long Position
	{
		get
		{
			return this.m_Compressed ? this.m_Uncomp.Position : this.RawStream.Position;
		}
		set
		{
			if (this.m_Compressed)
			{
				this.m_Uncomp.Position = value;
			}
			else
			{
				this.RawStream.Position = value;
			}
		}
	}

	public bool EndOfFile => (!this.m_Compressed || this.m_Uncomp.Position >= this.m_Uncomp.Length) && this.RawStream.Position >= this.RawStream.Length;

	public GZBlockIn(Stream underlyingStream)
	{
		this.m_Compressed = true;
		this.m_In = new BinaryReader(underlyingStream);
		this.m_Uncomp = new MemoryStream();
		this.m_Self = new BinaryReader(this);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException();
	}

	public override void Flush()
	{
		this.RawStream.Flush();
		this.m_Uncomp.Flush();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		if (this.m_Compressed)
		{
			long num = offset;
			if (origin == SeekOrigin.Current)
			{
				num += this.m_Uncomp.Position;
			}
			if (num < 0)
			{
				throw new Exception("Cannot seek past the begining of the stream.");
			}
			long position = this.m_Uncomp.Position;
			this.m_Uncomp.Seek(0L, SeekOrigin.End);
			while ((origin == SeekOrigin.End || num >= this.m_Uncomp.Length) && this.RawStream.Position < this.RawStream.Length)
			{
				int num2 = this.Raw.ReadInt32();
				int destLength = this.Raw.ReadInt32();
				if (GZBlockIn.m_ReadBuff == null || GZBlockIn.m_ReadBuff.Length < num2)
				{
					GZBlockIn.m_ReadBuff = new byte[num2];
				}
				if (GZBlockIn.m_CompBuff == null || GZBlockIn.m_CompBuff.Length < destLength)
				{
					GZBlockIn.m_CompBuff = new byte[destLength];
				}
				else
				{
					destLength = GZBlockIn.m_CompBuff.Length;
				}
				this.Raw.Read(GZBlockIn.m_ReadBuff, 0, num2);
				Z_RESULT z_RESULT = ZLib.Decompress(GZBlockIn.m_CompBuff, ref destLength, GZBlockIn.m_ReadBuff, num2);
				if (z_RESULT != Z_RESULT.Z_OK)
				{
					throw new Exception("ZLib error uncompressing: " + z_RESULT);
				}
				this.m_Uncomp.Write(GZBlockIn.m_CompBuff, 0, destLength);
			}
			this.m_Uncomp.Position = position;
			return this.m_Uncomp.Seek(offset, origin);
		}
		return this.RawStream.Seek(offset, origin);
	}

	public override void SetLength(long value)
	{
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (this.m_Compressed)
		{
			long position = this.m_Uncomp.Position;
			this.m_Uncomp.Seek(0L, SeekOrigin.End);
			while (position + count > this.m_Uncomp.Length && this.RawStream.Position + 8 < this.RawStream.Length)
			{
				int num = this.Raw.ReadInt32();
				int destLength = this.Raw.ReadInt32();
				if (num > 268435456 || num <= 0 || destLength > 268435456 || destLength <= 0 || this.RawStream.Position + num > this.RawStream.Length)
				{
					break;
				}
				if (GZBlockIn.m_ReadBuff == null || GZBlockIn.m_ReadBuff.Length < num)
				{
					GZBlockIn.m_ReadBuff = new byte[num];
				}
				if (GZBlockIn.m_CompBuff == null || GZBlockIn.m_CompBuff.Length < destLength)
				{
					GZBlockIn.m_CompBuff = new byte[destLength];
				}
				else
				{
					destLength = GZBlockIn.m_CompBuff.Length;
				}
				this.Raw.Read(GZBlockIn.m_ReadBuff, 0, num);
				Z_RESULT z_RESULT = ZLib.Decompress(GZBlockIn.m_CompBuff, ref destLength, GZBlockIn.m_ReadBuff, num);
				if (z_RESULT != Z_RESULT.Z_OK)
				{
					throw new Exception("ZLib error uncompressing: " + z_RESULT);
				}
				this.m_Uncomp.Write(GZBlockIn.m_CompBuff, 0, destLength);
			}
			this.m_Uncomp.Position = position;
			return this.m_Uncomp.Read(buffer, offset, count);
		}
		return this.RawStream.Read(buffer, offset, count);
	}

	public override void Close()
	{
		this.m_In.Close();
		this.m_Uncomp.Close();
		this.m_Self = null;
	}
}
