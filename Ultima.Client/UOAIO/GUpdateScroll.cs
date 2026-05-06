namespace UOAIO;

public class GUpdateScroll : GBackground
{
	public GUpdateScroll(string text)
		: base(5058, 100, 100, 40, 30, HasBorder: true)
	{
		GLabel gLabel = new GLabel("Updates", Engine.DefaultFont, Hues.Load(496), base.OffsetX, base.OffsetY);
		GBackground gBackground = new GBackground(3004, 100, 100, base.OffsetX, gLabel.Y + gLabel.Height + 4, HasBorder: true);
		GWrappedLabel gWrappedLabel = new GWrappedLabel(text, Engine.GetFont(1), Hues.Load(1109), gBackground.OffsetX + 2, gBackground.OffsetY + 2, 250);
		gBackground.Width = gBackground.Width - gBackground.UseWidth + gWrappedLabel.Width + 6;
		gBackground.Height = gBackground.Height - gBackground.UseHeight + gWrappedLabel.Height + 2;
		gBackground.Children.Add(gWrappedLabel);
		this.Width = this.Width - base.UseWidth + gBackground.Width;
		this.Height = this.Height - base.UseHeight + gLabel.Height + 4 + gBackground.Height;
		gLabel.X += (base.UseWidth - gLabel.Width) / 2;
		base.m_CanDrag = true;
		base.m_QuickDrag = true;
		base.CanClose = true;
		gBackground.SetMouseOverride(this);
		base.m_Children.Add(gLabel);
		base.m_Children.Add(gBackground);
	}
}
