using System;

namespace UOAIO;

public class GQueueStatus : GLabel
{
	private bool m_IsVisible;

	private DateTime m_HideAt;

	public override int Y
	{
		get
		{
			return Stats.yOffset;
		}
		set
		{
		}
	}

	public GQueueStatus()
		: base("", Engine.DefaultFont, Hues.Load(53), 4, 4)
	{
	}

	protected internal override void Render(int X, int Y)
	{
		int count = ActionContext.Queued.Count;
		if (count > 1)
		{
			this.m_IsVisible = true;
			this.m_HideAt = DateTime.MinValue;
		}
		else if (this.m_IsVisible && count == 0)
		{
			if (this.m_HideAt == DateTime.MinValue)
			{
				this.m_HideAt = DateTime.Now + TimeSpan.FromSeconds(1.0);
			}
			else if (DateTime.Now >= this.m_HideAt)
			{
				this.m_HideAt = DateTime.MinValue;
				this.m_IsVisible = false;
			}
		}
		if (Engine.m_Ingame && this.m_IsVisible)
		{
			if (count == 0)
			{
				this.Text = "Complete";
			}
			else
			{
				this.Text = $"Queued: {count:N0}";
			}
			base.Render(X, Y);
			Stats.Add(this);
		}
	}
}
