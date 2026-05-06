namespace UOAIO;

public class MobileFlags
{
	private int m_Value;

	private Mobile m_Target;

	public int Value
	{
		get
		{
			return this.m_Value;
		}
		set
		{
			this.m_Value = value;
			this.m_Target.OnFlagsChanged();
		}
	}

	public bool this[MobileFlag flag]
	{
		get
		{
			return ((uint)this.m_Value & (uint)flag) != 0;
		}
		set
		{
			if (value)
			{
				this.m_Value |= (int)flag;
			}
			else
			{
				this.m_Value &= (int)(~flag);
			}
			this.m_Target.OnFlagsChanged();
		}
	}

	public MobileFlags(Mobile who)
	{
		this.m_Target = who;
	}

	public MobileFlags Clone()
	{
		MobileFlags mobileFlags = new MobileFlags(this.m_Target);
		mobileFlags.m_Value = this.m_Value;
		return mobileFlags;
	}

	public override string ToString()
	{
		if ((this.m_Value & -224) != 0)
		{
			return $"0x{this.m_Value:X2}";
		}
		MobileFlag value = (MobileFlag)this.m_Value;
		return value.ToString();
	}
}
