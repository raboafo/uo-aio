namespace UOAIO;

public class Fade
{
	protected int m_Color;

	protected double m_From;

	protected double m_Delta;

	protected TimeSync m_Sync;

	public int Color => this.m_Color;

	public Fade(int Color, float From, float To, float Duration)
	{
		this.m_Color = Color;
		this.m_From = From;
		this.m_Delta = To - From;
		this.m_Sync = new TimeSync(Duration);
	}

	public virtual bool Evaluate(ref double Alpha)
	{
		double normalized = this.m_Sync.Normalized;
		Alpha = this.m_From + this.m_Delta * normalized;
		return normalized < 1.0;
	}

	protected internal virtual void OnFadeComplete()
	{
	}
}
