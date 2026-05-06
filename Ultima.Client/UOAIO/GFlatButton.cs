namespace UOAIO;

public class GFlatButton : GAlphaBackground
{
	protected OnClick m_OnClick;

	private void Route_OnClick(Gump g)
	{
		if (this.m_OnClick != null)
		{
			this.m_OnClick(this);
		}
	}

	public void Click()
	{
		if (this.m_OnClick != null)
		{
			this.m_OnClick(this);
		}
	}

	public GFlatButton(int X, int Y, int Width, int Height, string Text, OnClick OnClick)
		: base(X, Y, Width, Height)
	{
		this.m_OnClick = OnClick;
		base.m_CanDrag = false;
		GTextButton gTextButton = new GTextButton(Text, Engine.GetUniFont(0), Hues.Default, Hues.Load(53), 0, 0, Route_OnClick);
		base.m_Children.Add(gTextButton);
		gTextButton.Center();
		base.m_Children.Add(new GHotspot(0, 0, Width, Height, gTextButton));
	}
}
