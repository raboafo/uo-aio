using System;

namespace UOAIO;

public static class TextureEffects
{
	private static byte[] _screenTable;

	private static byte[] GetScreenTable()
	{
		if (TextureEffects._screenTable == null)
		{
			TextureEffects._screenTable = new byte[1024];
			for (int i = 0; i < 32; i++)
			{
				for (int j = 0; j < 32; j++)
				{
					double num = (double)i / 31.0;
					double num2 = (double)j / 31.0;
					double num3 = 1.0 - (1.0 - num) * (1.0 - num2);
					num3 = num * 0.25 + num3 * 0.5 + num2 * 0.25;
					num3 += Math.Sqrt(num) * 0.1;
					int num4 = (int)(num3 * 31.0 + 0.5);
					if (num4 < 0)
					{
						num4 = 0;
					}
					else if (num4 > 31)
					{
						num4 = 31;
					}
					TextureEffects._screenTable[(i << 5) | j] = (byte)num4;
				}
			}
		}
		return TextureEffects._screenTable;
	}

	public unsafe static void MedianBlur(ushort* pLine, ushort* pLineEnd, ushort* pImageEnd, int lineDelta, int lineEndDelta, int width, int height)
	{
	}

	private unsafe static ushort Median(ushort* pInput, int x, int y, int width, int height, int stride)
	{
		ushort* ptr = stackalloc ushort[32];
		ushort* ptr2 = stackalloc ushort[32];
		ushort* ptr3 = stackalloc ushort[32];
		int num = 0;
		for (int i = -16; i <= 16; i++)
		{
			for (int j = -16; j <= 16; j++)
			{
				if (i * i + j * j >= 256)
				{
					continue;
				}
				int num2 = x + j;
				int num3 = y + i;
				if (num2 >= 0 && num2 < width && num3 >= 0 && num3 < height)
				{
					ushort num4 = pInput[num3 * stride + num2];
					if ((num4 & 0x8000) == 32768)
					{
						ushort* num5 = ptr + ((num4 >> 10) & 0x1F);
						(*num5)++;
						ushort* num6 = ptr2 + ((num4 >> 5) & 0x1F);
						(*num6)++;
						ushort* num7 = ptr3 + (num4 & 0x1F);
						(*num7)++;
						num++;
					}
				}
			}
		}
		if (num == 0)
		{
			return 0;
		}
		ushort num8 = 0;
		ushort num9 = 0;
		ushort num10 = 0;
		int num11 = num / 8;
		ushort num12 = 0;
		ushort num13 = 0;
		ushort num14 = 0;
		while (num8 < 31 && num12 < num11)
		{
			num12 += ptr[(int)num8++];
		}
		while (num9 < 31 && num13 < num11)
		{
			num13 += ptr2[(int)num9++];
		}
		while (num10 < 31 && num14 < num11)
		{
			num14 += ptr3[(int)num10++];
		}
		return (ushort)(0x8000 | (num8 << 10) | (num9 << 5) | num10);
	}
}
