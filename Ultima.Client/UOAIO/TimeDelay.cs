namespace UOAIO;

public class TimeDelay
{
	private int m_End;

	private int m_Duration;

	public bool Elapsed => Engine.Ticks >= this.m_End;

	public TimeDelay(float Duration)
	{
		this.m_Duration = (int)(Duration * 1000f);
		this.m_End = Engine.Ticks + this.m_Duration;
	}

	public TimeDelay(int msDuration)
	{
		this.m_Duration = msDuration;
		this.m_End = Engine.Ticks + this.m_Duration;
	}

	public void Reset()
	{
		this.m_End = Engine.Ticks + this.m_Duration;
	}

	public bool ElapsedReset()
	{
		int ticks = Engine.Ticks;
		if (ticks >= this.m_End)
		{
			this.m_End = ticks + this.m_Duration;
			return true;
		}
		return false;
	}
}
