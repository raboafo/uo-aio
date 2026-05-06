using System;

namespace UOAIO;

public class Volume
{
	public const int Minimum = 0;

	public const int Maximum = 10000;

	public const int Range = 10000;

	public static readonly double MaxDB;

	public static readonly double MaxIV;

	public static readonly double MaxXP;

	private int m_Scale;

	private bool m_Mute;

	private Callback m_OnChange;

	public Callback OnChange
	{
		get
		{
			return this.m_OnChange;
		}
		set
		{
			this.m_OnChange = value;
		}
	}

	public int Scale
	{
		get
		{
			return this.m_Scale;
		}
		set
		{
			value = Volume.ApplyBounds(value);
			if (this.m_Scale != value)
			{
				this.m_Scale = value;
				if (this.m_OnChange != null)
				{
					this.m_OnChange();
				}
			}
		}
	}

	public bool Mute
	{
		get
		{
			return this.m_Mute;
		}
		set
		{
			if (this.m_Mute != value)
			{
				this.m_Mute = value;
				if (this.m_OnChange != null)
				{
					this.m_OnChange();
				}
			}
		}
	}

	public int Value => Volume.ApplyBounds(Volume.FromScale(this.m_Scale));

	public bool IsMuted => this.m_Mute || this.m_Scale <= 0;

	public static int ApplyBounds(int value)
	{
		return Math.Min(Math.Max(value, 0), 10000);
	}

	public static int FromScale(int scale)
	{
		double num = (double)Volume.ApplyBounds(scale) / 10000.0;
		if (num == 0.0)
		{
			return 0;
		}
		double a = Math.Exp(Volume.MaxXP * num) / Volume.MaxIV * 10000.0;
		return (int)Math.Round(a);
	}

	public Volume()
	{
		this.m_Mute = false;
		this.m_Scale = 10000;
	}

	static Volume()
	{
		Volume.MaxDB = 3.25;
		Volume.MaxIV = Math.Pow(10.0, Volume.MaxDB / 20.0);
		Volume.MaxXP = Math.Log(Volume.MaxIV);
	}
}
