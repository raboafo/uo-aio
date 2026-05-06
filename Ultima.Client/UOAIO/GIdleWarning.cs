using System;

namespace UOAIO;

public class GIdleWarning : GBackground
{
	public GIdleWarning()
		: base(2604, 100, 100, 0, 0, HasBorder: true)
	{
		base.m_CanDrag = true;
		base.m_QuickDrag = true;
		GWrappedLabel gWrappedLabel = new GWrappedLabel("You have been idle for too long. If you do not do anything in the next minute, you will be logged out.", Engine.GetFont(2), Hues.Load(1109), base.OffsetX, base.OffsetY - 12, 275);
		base.m_Children.Add(gWrappedLabel);
		GButtonNew gButtonNew = new GButtonNew(1153, 0, gWrappedLabel.Y + gWrappedLabel.Height + 4);
		gButtonNew.Clicked += Check_Clicked;
		base.m_Children.Add(gButtonNew);
		this.Width = this.Width - base.UseWidth + gWrappedLabel.Width;
		this.Height = this.Height - base.UseHeight + gWrappedLabel.Height - 12;
		gButtonNew.X = base.OffsetX + (base.UseWidth - gButtonNew.Width) / 2;
		this.Center();
	}

	private void Check_Clicked(object sender, EventArgs e)
	{
		Gumps.Destroy(this);
	}
}
