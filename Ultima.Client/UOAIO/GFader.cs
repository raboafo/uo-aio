namespace UOAIO;

public class GFader : GDragable
{
	protected static bool m_Fade;

	private float m_fDuration;

	private float m_fFadeInDuration;

	private float m_fFadeTo;

	private float m_FadeTo;

	private float m_FadeFrom;

	private FadeState m_State;

	private TimeSync m_Sync;

	private Timer m_Timer;

	public static bool Fade
	{
		get
		{
			return GFader.m_Fade;
		}
		set
		{
			GFader.m_Fade = value;
		}
	}

	public FadeState State
	{
		get
		{
			return this.m_State;
		}
		set
		{
			if (this.m_State == value)
			{
				return;
			}
			switch (value)
			{
			case FadeState.Faded:
				base.Alpha = this.m_fFadeTo;
				if (this.m_Timer != null)
				{
					this.m_Timer.Delete();
				}
				this.m_Sync = null;
				this.m_Timer = null;
				break;
			case FadeState.Opaque:
				base.Alpha = 1f;
				if (this.m_Timer != null)
				{
					this.m_Timer.Delete();
				}
				this.m_Sync = null;
				this.m_Timer = null;
				break;
			case FadeState.O2F:
				this.m_FadeTo = this.m_fFadeTo;
				this.m_FadeFrom = base.Alpha;
				if (this.m_Timer != null)
				{
					this.m_Timer.Delete();
				}
				this.m_Timer = new Timer(Fade_OnTick, 0);
				this.m_Sync = new TimeSync(this.m_fDuration);
				this.m_Timer.Start(Now: true);
				break;
			case FadeState.F2O:
				this.m_FadeTo = 1f;
				this.m_FadeFrom = base.Alpha;
				if (this.m_Timer != null)
				{
					this.m_Timer.Delete();
				}
				this.m_Timer = new Timer(Fade_OnTick, 0);
				this.m_Sync = new TimeSync(this.m_fFadeInDuration);
				this.m_Timer.Start(Now: true);
				break;
			}
			this.m_State = value;
		}
	}

	public GFader(float Duration, float FadeInDuration, float FadeTo, int GumpID, int X, int Y)
		: this(Duration, FadeInDuration, FadeTo, GumpID, X, Y, Hues.Default)
	{
	}

	public GFader(float Duration, float FadeInDuration, float FadeTo, int GumpID, int X, int Y, IHue h)
		: base(GumpID, h, X, Y)
	{
		this.m_fDuration = Duration;
		this.m_fFadeTo = FadeTo;
		this.m_fFadeInDuration = FadeInDuration;
		this.State = FadeState.O2F;
	}

	protected internal override void Render(int X, int Y)
	{
		if (!base.m_Visible)
		{
			return;
		}
		if (!GFader.m_Fade)
		{
			base.Alpha = 1f;
			this.m_State = FadeState.Opaque;
			base.Render(X, Y);
			return;
		}
		int x = X + this.X;
		int y = Y + this.Y;
		this.Draw(x, y);
		Gump[] array = base.m_Children.ToArray();
		float num = 0f;
		foreach (Gump gump in array)
		{
			if (gump.m_ITranslucent)
			{
				ITranslucent translucent = (ITranslucent)gump;
				num = translucent.Alpha;
				translucent.Alpha *= base.m_fAlpha;
				gump.Render(x, y);
				translucent.Alpha = num;
			}
			else
			{
				gump.Render(x, y);
			}
		}
	}

	protected void Fade_OnTick(Timer t)
	{
		if (this.m_Sync != null)
		{
			double normalized = this.m_Sync.Normalized;
			base.Alpha = this.m_FadeFrom + (float)((double)(this.m_FadeTo - this.m_FadeFrom) * normalized);
			if (normalized >= 1.0)
			{
				this.m_Sync = null;
				if (this.State == FadeState.F2O)
				{
					this.State = FadeState.Opaque;
				}
				else if (this.State == FadeState.O2F)
				{
					this.State = FadeState.Faded;
				}
			}
		}
		else
		{
			t.Delete();
			this.m_Timer = null;
		}
	}

	protected internal override void Draw(int X, int Y)
	{
		if (GFader.m_Fade)
		{
			if (Gumps.LastOver == null || !Gumps.LastOver.IsChildOf(this))
			{
				if (this.State != FadeState.O2F && this.State != FadeState.Faded)
				{
					this.State = FadeState.O2F;
				}
			}
			else if (this.State != FadeState.F2O && this.State != FadeState.Opaque)
			{
				this.State = FadeState.F2O;
			}
		}
		base.Draw(X, Y);
	}
}
