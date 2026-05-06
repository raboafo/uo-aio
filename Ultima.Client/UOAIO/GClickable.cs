using System;

namespace UOAIO;

public class GClickable : GImage
{
	public event EventHandler Clicked;

	public event EventHandler DoubleClicked;

	public GClickable(int x, int y, int gumpID)
		: base(gumpID, x, y)
	{
	}

	public GClickable(int x, int y, int gumpID, IHue hue)
		: base(gumpID, hue, x, y)
	{
	}

	protected internal override bool HitTest(int x, int y)
	{
		if (base.m_Invalidated)
		{
			base.Refresh();
		}
		return base.m_Draw && base.m_Image.HitTest(x, y);
	}

	protected internal override void OnSingleClick(int x, int y)
	{
		this.InternalOnSingleClicked();
	}

	protected internal override void OnDoubleClick(int x, int y)
	{
		this.InternalOnDoubleClicked();
	}

	private void InternalOnSingleClicked()
	{
		this.OnSingleClicked();
		if (this.Clicked != null)
		{
			this.Clicked(this, EventArgs.Empty);
		}
	}

	protected virtual void OnSingleClicked()
	{
	}

	private void InternalOnDoubleClicked()
	{
		this.OnDoubleClicked();
		if (this.DoubleClicked != null)
		{
			this.DoubleClicked(this, EventArgs.Empty);
		}
	}

	protected virtual void OnDoubleClicked()
	{
	}
}
