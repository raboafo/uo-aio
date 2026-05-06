namespace UOAIO;

public class TimeSync
{
	private double m_Duration;

	private double m_Start;

	private double m_rSeconds;

	public double Elapsed
	{
		get
		{
			double num = Engine.m_dTicks;
			if (!Engine.m_SetTicks)
			{
				num = Engine.UpdateTicks();
			}
			double num2 = num - this.m_Start;
			double num3 = num2 / 1000.0;
			if (num3 < 0.0)
			{
				num3 = 0.0;
			}
			return num3;
		}
	}

	public double Normalized
	{
		get
		{
			double num = Engine.m_dTicks;
			if (!Engine.m_SetTicks)
			{
				num = Engine.UpdateTicks();
			}
			double num2 = num - this.m_Start;
			double num3 = num2 * this.m_rSeconds;
			if (num3 < 0.0)
			{
				num3 = 0.0;
			}
			else if (num3 > 1.0)
			{
				num3 = 1.0;
			}
			return num3;
		}
	}

	public TimeSync(double duration)
	{
		this.Initialize(duration);
	}

	public void Initialize(double duration)
	{
		this.m_Duration = duration;
		this.m_Start = Engine.m_dTicks;
		if (!Engine.m_SetTicks)
		{
			this.m_Start = Engine.UpdateTicks();
		}
		this.m_rSeconds = 1.0 / (duration * 1000.0);
	}
}
