namespace UOAIO;

public class TileGroup
{
	private int m_Start;

	private int m_Count;

	public TileGroup(int start, int count)
	{
		this.m_Start = start;
		this.m_Count = count;
	}

	public unsafe static int GetBrightness(Texture tex, int xStart, int yStart, int xStep, int yStep, int count)
	{
		LockData lockData = tex.Lock(LockFlags.ReadOnly);
		short* pvSrc = (short*)lockData.pvSrc;
		int num = lockData.Pitch >> 1;
		pvSrc += yStart * num;
		pvSrc += xStart;
		num *= yStep;
		num += xStep;
		int num2 = 0;
		for (int i = 0; i < count; i++)
		{
			short num3 = *pvSrc;
			num2 += (num3 & 0x1F) * 114;
			num2 += ((num3 >> 5) & 0x1F) * 587;
			num2 += ((num3 >> 10) & 0x1F) * 299;
			pvSrc += num;
		}
		tex.Unlock();
		num2 <<= 3;
		num2 /= count;
		return num2 / 1000;
	}
}
