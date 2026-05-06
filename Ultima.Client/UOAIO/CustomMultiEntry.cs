using System;
using System.Collections.Generic;
using Veritas.Compression;

namespace UOAIO;

public class CustomMultiEntry
{
	private int m_Serial;

	private int m_Revision;

	private Multi m_Multi;

	private static byte[] m_InflateBuffer;

	public Multi Multi => this.m_Multi;

	public int Serial => this.m_Serial;

	public int Revision => this.m_Revision;

	public static void LoadUncompressed(byte[] buffer, List<MultiItem> list)
	{
		int num = buffer.Length / 5;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			int num3 = (buffer[num2++] << 8) | buffer[num2++];
			int num4 = (sbyte)buffer[num2++];
			int num5 = (sbyte)buffer[num2++];
			int num6 = (sbyte)buffer[num2++];
			list.Add(new MultiItem
			{
				Flags = 1,
				ItemID = (ushort)num3,
				X = (short)num4,
				Y = (short)num5,
				Z = (short)num6
			});
		}
	}

	public unsafe static void LoadDeflated(int xMin, int yMin, int xMax, int yMax, byte[] buffer, List<MultiItem> list)
	{
		int num = yMax - yMin + 1;
		fixed (byte* ptr = buffer)
		{
			byte* ptr2 = ptr;
			int num2 = *(ptr2++);
			for (int i = 0; i < num2; i++)
			{
				int num3 = (*ptr2 >> 4) & 0xF;
				int num4 = *ptr2 & 0xF;
				int destLength = ptr2[1] | ((ptr2[3] << 4) & 0xF00);
				int num5 = ptr2[2] | ((ptr2[3] << 8) & 0xF00);
				ptr2 += 4;
				if (CustomMultiEntry.m_InflateBuffer == null || CustomMultiEntry.m_InflateBuffer.Length < destLength)
				{
					CustomMultiEntry.m_InflateBuffer = new byte[destLength];
				}
				fixed (byte* inflateBuffer = CustomMultiEntry.m_InflateBuffer)
				{
					byte* ptr3 = inflateBuffer;
					byte[] array = new byte[num5];
					for (int j = 0; j < array.Length; j++)
					{
						array[j] = *(ptr2++);
					}
					ZLib.Decompress(CustomMultiEntry.m_InflateBuffer, ref destLength, array, array.Length);
					byte* ptr4 = ptr3 + destLength;
					switch (num3)
					{
					case 0:
						while (ptr3 < ptr4)
						{
							MultiItem item = new MultiItem
							{
								Flags = 1,
								ItemID = (ushort)((*ptr3 << 8) | ptr3[1]),
								X = (sbyte)ptr3[2],
								Y = (sbyte)ptr3[3],
								Z = (sbyte)ptr3[4]
							};
							ptr3 += 5;
							if (item.ItemID != 0)
							{
								list.Add(item);
							}
						}
						break;
					case 1:
					{
						int num12 = 0;
						switch (num4)
						{
						case 0:
							num12 = 0;
							break;
						case 1:
						case 5:
							num12 = 7;
							break;
						case 2:
						case 6:
							num12 = 27;
							break;
						case 3:
						case 7:
							num12 = 47;
							break;
						case 4:
						case 8:
							num12 = 67;
							break;
						}
						while (ptr3 < ptr4)
						{
							MultiItem item2 = new MultiItem
							{
								Flags = 1,
								ItemID = (ushort)((*ptr3 << 8) | ptr3[1]),
								X = (sbyte)ptr3[2],
								Y = (sbyte)ptr3[3],
								Z = (sbyte)num12
							};
							ptr3 += 4;
							if (item2.ItemID != 0)
							{
								list.Add(item2);
							}
						}
						break;
					}
					case 2:
					{
						int num6 = 0;
						switch (num4)
						{
						case 0:
							num6 = 0;
							break;
						case 1:
						case 5:
							num6 = 7;
							break;
						case 2:
						case 6:
							num6 = 27;
							break;
						case 3:
						case 7:
							num6 = 47;
							break;
						case 4:
						case 8:
							num6 = 67;
							break;
						}
						int num7;
						int num8;
						int num9;
						if (num4 <= 0)
						{
							num7 = xMin;
							num8 = yMin;
							num9 = num + 1;
						}
						else if (num4 <= 4)
						{
							num7 = xMin + 1;
							num8 = yMin + 1;
							num9 = num - 1;
						}
						else
						{
							num7 = xMin;
							num8 = yMin;
							num9 = num;
						}
						int num10 = 0;
						while (ptr3 < ptr4)
						{
							ushort num11 = (ushort)((*ptr3 << 8) | ptr3[1]);
							num10++;
							ptr3 += 2;
							if (num11 != 0)
							{
								list.Add(new MultiItem
								{
									Flags = 1,
									ItemID = num11,
									X = (short)(num7 + (num10 - 1) / num9),
									Y = (sbyte)(num8 + (num10 - 1) % num9),
									Z = (sbyte)num6
								});
							}
						}
						break;
					}
					}
				}
			}
		}
	}

	public CustomMultiEntry(int ser, int rev, Multi baseMulti, int compressionType, byte[] buffer)
	{
		this.m_Serial = ser;
		this.m_Revision = rev;
		baseMulti.GetBounds(out var xMin, out var yMin, out var xMax, out var yMax);
		List<MultiItem> list = new List<MultiItem>();
		try
		{
			switch (compressionType)
			{
			case 0:
				CustomMultiEntry.LoadUncompressed(buffer, list);
				break;
			case 3:
				CustomMultiEntry.LoadDeflated(xMin, yMin, xMax, yMax, buffer, list);
				break;
			}
		}
		catch (Exception ex)
		{
			Debug.Error(ex);
		}
		this.m_Multi = new Multi(list);
	}
}
