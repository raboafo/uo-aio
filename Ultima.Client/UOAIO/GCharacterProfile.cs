namespace UOAIO;

public class GCharacterProfile : GBackground
{
	public GCharacterProfile(Mobile owner, string header, string body, string footer)
		: base(5058, 100, 100, 25, 25, HasBorder: true)
	{
		string gUID = $"Profile-{owner.Serial}";
		Gump gump = Gumps.FindGumpByGUID(gUID);
		if (gump != null)
		{
			base.m_IsDragging = gump.m_IsDragging;
			base.m_OffsetX = gump.m_OffsetX;
			base.m_OffsetY = gump.m_OffsetY;
			if (Gumps.Drag == gump)
			{
				Gumps.Drag = this;
			}
			if (Gumps.LastOver == gump)
			{
				Gumps.LastOver = this;
			}
			if (Gumps.Focus == gump)
			{
				Gumps.Focus = this;
			}
			base.m_X = gump.X;
			base.m_Y = gump.Y;
			Gumps.Destroy(gump);
		}
		base.m_GUID = gUID;
		base.m_CanDrag = true;
		base.m_QuickDrag = true;
		base.CanClose = true;
		Gump gump2 = this.CreateLabel(header, scroll: false);
		Gump gump3 = this.CreateLabel(body, scroll: true);
		Gump gump4 = this.CreateLabel(footer, scroll: false);
		gump2.X = base.OffsetX;
		gump2.Y = base.OffsetY;
		gump3.X = gump2.X;
		gump3.Y = gump2.Y + gump2.Height;
		gump4.X = gump3.X;
		gump4.Y = gump3.Y + gump3.Height;
		this.Height = this.Height - base.UseHeight + gump2.Height + gump3.Height + gump4.Height;
		this.Width = gump2.Width;
		if (gump3.Width > this.Width)
		{
			this.Width = gump3.Width;
		}
		if (gump4.Width > this.Width)
		{
			this.Width = gump4.Width;
		}
		this.Width += this.Width - base.UseWidth;
		base.m_Children.Add(gump2);
		base.m_Children.Add(gump3);
		base.m_Children.Add(gump4);
	}

	private Gump CreateLabel(string text, bool scroll)
	{
		text = text.Replace('\r', '\n');
		GBackground gBackground = new GBackground(3004, 200, 100, HasBorder: true);
		GWrappedLabel gWrappedLabel = new GWrappedLabel(text, Engine.GetFont(1), Hues.Load(1109), gBackground.OffsetX, gBackground.OffsetY, gBackground.UseWidth);
		gBackground.Height = gWrappedLabel.Height + (gBackground.Height - gBackground.UseHeight);
		gBackground.Children.Add(gWrappedLabel);
		gWrappedLabel.Center();
		gBackground.SetMouseOverride(this);
		return gBackground;
	}
}
