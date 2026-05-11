using System;
using System.IO;
using System.Text;

namespace UOAIO;

public class PacketReader
{
	private byte[] m_Data;

	private int m_Start;

	private int m_Index;

	private int m_Count;

	private int m_Bounds;

	private bool m_FixedSize;

	private byte m_Command;

	private string m_Name;

	private string m_ReturnName;

	private static PacketReader m_Instance;

	public string ReturnName
	{
		get
		{
			return this.m_ReturnName;
		}
		set
		{
			this.m_ReturnName = value;
		}
	}

	public bool Finished => this.m_Index >= this.m_Bounds;

	public byte Command => this.m_Command;

	public string Name => this.m_Name;

	public bool FixedSize => this.m_FixedSize;

	public int Position
	{
		get
		{
			return this.m_Index - this.m_Start;
		}
		set
		{
			this.m_Index = value + this.m_Start;
		}
	}

	public int Start => this.m_Start;

	public int Length => this.m_Count;

	public static PacketReader Initialize(byte[] data, int index, int count, bool fixedSize, byte command, string name)
	{
		if (PacketReader.m_Instance == null)
		{
			PacketReader.m_Instance = new PacketReader(data, index, count, fixedSize, command, name);
		}
		else
		{
			PacketReader.m_Instance.m_Data = data;
			PacketReader.m_Instance.m_Start = (PacketReader.m_Instance.m_Index = index);
			PacketReader.m_Instance.m_Count = count;
			PacketReader.m_Instance.m_Bounds = PacketReader.m_Instance.m_Start + PacketReader.m_Instance.m_Count;
			PacketReader.m_Instance.m_FixedSize = fixedSize;
			PacketReader.m_Instance.m_Command = command;
			PacketReader.m_Instance.m_Name = name;
			PacketReader.m_Instance.m_ReturnName = name;
			if (!fixedSize)
			{
				PacketReader.m_Instance.m_Index += 3;
			}
			else
			{
				PacketReader.m_Instance.m_Index++;
			}
		}
		return PacketReader.m_Instance;
	}

	public PacketReader(byte[] data, int index, int count, bool fixedSize, byte command, string name)
	{
		this.m_Data = data;
		this.m_Start = (this.m_Index = index);
		this.m_Count = count;
		this.m_Bounds = this.m_Start + this.m_Count;
		this.m_FixedSize = fixedSize;
		this.m_Command = command;
		this.m_Name = name;
		this.m_ReturnName = name;
		if (!fixedSize)
		{
			this.m_Index += 3;
		}
		else
		{
			this.m_Index++;
		}
	}

	public void Trace(bool silent = false)
	{
		if (!silent)
		{
			Engine.AddTextMessage(string.Format("Tracing packet 0x{0:X2} '{1}' of length {2} ( 0x{2:X} ). (Prior: 0x{3:X2}, 0x{4:X2}, 0x{5:X2})", this.m_Command, this.m_Name, this.m_Count, NetworkContext.prior1, NetworkContext.prior2, NetworkContext.prior3));
		}
		StreamWriter streamWriter = ClientRuntimeEnvironment.CreateRuntimeTextWriter("data/ultima/logs/PacketTrace.log", append: true);
		if (this.m_Count < 16)
		{
			streamWriter.WriteLine("Packet Server->Client '{0}' ( {1} bytes )", this.m_ReturnName, this.m_Count);
		}
		else
		{
			streamWriter.WriteLine("Packet Server->Client '{0}' ( {1} [0x{1:X}] bytes )", this.m_ReturnName, this.m_Count);
		}
		streamWriter.WriteLine();
		PacketLogger.WriteBuffer(streamWriter, this.m_Data, this.m_Start, this.m_Count);
		streamWriter.WriteLine();
		streamWriter.Flush();
		streamWriter.Close();
	}

	public void Seek(int offset, SeekOrigin origin)
	{
		switch (origin)
		{
		case SeekOrigin.Begin:
			this.m_Index = this.m_Start + offset;
			break;
		case SeekOrigin.Current:
			this.m_Index += offset;
			break;
		case SeekOrigin.End:
			this.m_Index = this.m_Bounds + offset;
			break;
		}
	}

	public byte[] ReadBytes(int length)
	{
		byte[] array = new byte[length];
		Buffer.BlockCopy(this.m_Data, this.m_Index, array, 0, length);
		this.m_Index += length;
		return array;
	}

	public unsafe string ReadString()
	{
		fixed (byte* data = this.m_Data)
		{
			byte* ptr = data + this.m_Index;
			byte* ptr2 = data + this.m_Bounds;
			byte* ptr3 = ptr;
			byte* ptr4 = ptr;
			while ((ptr4 = ptr3) < ptr2 && *(ptr3++) != 0)
			{
			}
			this.m_Index = (int)(ptr4 - data + 1);
			return new string((sbyte*)ptr, 0, (int)(ptr4 - ptr));
		}
	}

	public unsafe string ReadUTF8()
	{
		fixed (byte* data = this.m_Data)
		{
			byte* ptr = data + this.m_Index;
			byte* ptr2 = data + this.m_Bounds;
			byte* ptr3 = ptr;
			byte* ptr4 = ptr;
			while ((ptr4 = ptr3) < ptr2 && *(ptr3++) != 0)
			{
			}
			int index = this.m_Index;
			this.m_Index = (int)(ptr4 - data + 1);
			return Encoding.UTF8.GetString(this.m_Data, index, (int)(ptr4 - ptr));
		}
	}

	public unsafe string ReadString(int fixedLength)
	{
		fixed (byte* data = this.m_Data)
		{
			byte* ptr = data + this.m_Index;
			byte* ptr2 = ptr + fixedLength;
			byte* ptr3 = ptr;
			byte* ptr4 = ptr;
			if (data + this.m_Bounds < ptr2)
			{
				ptr2 = data + this.m_Bounds;
			}
			while ((ptr4 = ptr3) < ptr2 && *(ptr3++) != 0)
			{
			}
			this.m_Index += fixedLength;
			return new string((sbyte*)ptr, 0, (int)(ptr4 - ptr));
		}
	}

	public unsafe string ReadUnicodeString()
	{
		fixed (byte* data = this.m_Data)
		{
			byte* ptr = data + this.m_Index;
			byte* ptr2 = data + this.m_Bounds;
			byte* ptr3 = ptr;
			byte* ptr4 = ptr;
			while ((ptr4 = ptr3) < ptr2 && (*(ptr3++) | *(ptr3++)) != 0)
			{
			}
			this.m_Index = (int)(ptr4 - data + 2);
			return new string((sbyte*)ptr, 0, (int)(ptr4 - ptr), Encoding.BigEndianUnicode);
		}
	}

	public unsafe string ReadUnicodeString(int fixedLength)
	{
		fixed (byte* data = this.m_Data)
		{
			byte* ptr = data + this.m_Index;
			byte* ptr2 = ptr + (fixedLength << 1);
			byte* ptr3 = ptr;
			byte* ptr4 = ptr;
			if (data + this.m_Bounds < ptr2)
			{
				ptr2 = data + this.m_Bounds;
			}
			while ((ptr4 = ptr3) < ptr2 && (*(ptr3++) | *(ptr3++)) != 0)
			{
			}
			this.m_Index += fixedLength << 1;
			return new string((sbyte*)ptr, 0, (int)(ptr4 - ptr), Encoding.BigEndianUnicode);
		}
	}

	public unsafe string ReadUnicodeLEString()
	{
		fixed (byte* data = this.m_Data)
		{
			byte* ptr = data + this.m_Index;
			byte* ptr2 = data + this.m_Bounds;
			byte* ptr3 = ptr;
			byte* ptr4 = ptr;
			while ((ptr4 = ptr3) < ptr2 && (*(ptr3++) | *(ptr3++)) != 0)
			{
			}
			this.m_Index = (int)(ptr4 - data + 2);
			return new string((sbyte*)ptr, 0, (int)(ptr4 - ptr), Encoding.Unicode);
		}
	}

	public bool ReadBoolean()
	{
		return this.m_Data[this.m_Index++] != 0;
	}

	public byte ReadByte()
	{
		return this.m_Data[this.m_Index++];
	}

	public sbyte ReadSByte()
	{
		return (sbyte)this.m_Data[this.m_Index++];
	}

	public short ReadInt16()
	{
		return (short)((this.m_Data[this.m_Index++] << 8) | this.m_Data[this.m_Index++]);
	}

	public ushort ReadUInt16()
	{
		return (ushort)((this.m_Data[this.m_Index++] << 8) | this.m_Data[this.m_Index++]);
	}

	public int ReadInt32()
	{
		return (this.m_Data[this.m_Index++] << 24) | (this.m_Data[this.m_Index++] << 16) | (this.m_Data[this.m_Index++] << 8) | this.m_Data[this.m_Index++];
	}

	public uint ReadUInt32()
	{
		return (uint)((this.m_Data[this.m_Index++] << 24) | (this.m_Data[this.m_Index++] << 16) | (this.m_Data[this.m_Index++] << 8) | this.m_Data[this.m_Index++]);
	}
}
