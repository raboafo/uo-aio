namespace UOAIO;

public class ItemFlags
{
	private int m_Value;

	public int Value
	{
		get
		{
			return this.m_Value;
		}
		set
		{
			this.m_Value = value;
			if ((this.m_Value & -161) != 0)
			{
				string message = $"Unknown item flags: 0x{this.m_Value:X2}";
				Debug.Trace(message);
				Engine.AddTextMessage(message);
			}
		}
	}

	public bool this[ItemFlag flag]
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
		}
	}

	public override string ToString()
	{
		if ((this.m_Value & -161) != 0)
		{
			return $"Unknown flags: 0x{this.m_Value:X2}";
		}
		ItemFlag value = (ItemFlag)this.m_Value;
		return value.ToString();
	}
}
