using System;

namespace UOAIO;

public class OptionRangeAttribute : Attribute
{
	private int m_Min;

	private int m_Max;

	public int Min => this.m_Min;

	public int Max => this.m_Max;

	public OptionRangeAttribute(int min, int max)
	{
		this.m_Min = min;
		this.m_Max = max;
	}
}
