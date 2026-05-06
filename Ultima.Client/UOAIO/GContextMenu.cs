using UOAIO.Profiles;

namespace UOAIO;

public class GContextMenu : Gump
{
	private int m_Width;

	private int m_Height;

	private object m_Owner;

	private static GContextMenu m_Instance;

	public override int X
	{
		get
		{
			int num = ((!(this.m_Owner is Mobile)) ? (((Item)this.m_Owner).MessageX - this.m_Width / 2) : (((Mobile)this.m_Owner).ScreenX - this.m_Width / 2));
			if (num < 0)
			{
				num = 0;
			}
			else if (num + this.m_Width > Engine.ScreenWidth)
			{
				num = Engine.ScreenWidth - this.m_Width;
			}
			return num;
		}
	}

	public override int Y
	{
		get
		{
			int num;
			if (this.m_Owner is Mobile)
			{
				Mobile mobile = (Mobile)this.m_Owner;
				num = mobile.ScreenY;
				if (Options.Current.MiniHealth && mobile.OpenedStatus && mobile.StatusBar == null && mobile.MaximumHitPoints > 0 && mobile.CurrentHitPoints > 0)
				{
					num += 11;
				}
				num += 8;
			}
			else
			{
				Item item = (Item)this.m_Owner;
				num = item.BottomY + 4;
			}
			if (num < 0)
			{
				num = 0;
			}
			else if (num + this.m_Height > Engine.ScreenHeight)
			{
				num = Engine.ScreenHeight - this.m_Height;
			}
			return num;
		}
	}

	public override int Width => this.m_Width;

	public override int Height => this.m_Height;

	public static void Close()
	{
		Gumps.Destroy(GContextMenu.m_Instance);
	}

	public static void Display(object owner, PopupEntry[] list)
	{
		Gumps.Destroy(GContextMenu.m_Instance);
		GContextMenu.m_Instance = new GContextMenu(owner, list);
		Gumps.Desktop.Children.Add(GContextMenu.m_Instance);
	}

	protected internal override bool HitTest(int X, int Y)
	{
		return false;
	}

	protected internal override void Draw(int X, int Y)
	{
		int num = this.m_Width - 24;
		int num2 = this.m_Height - 24;
		Engine.m_Edge[0].Draw(X, Y, 0);
		Engine.m_Edge[1].Draw(X + 12, Y, num, 12, 0);
		Engine.m_Edge[2].Draw(X + 12 + num, Y, 0);
		Engine.m_Edge[3].Draw(X, Y + 12, 12, num2, 0);
		Engine.m_Edge[4].Draw(X + 12 + num, Y + 12, 12, num2, 0);
		Engine.m_Edge[5].Draw(X, Y + 12 + num2, 0);
		Engine.m_Edge[6].Draw(X + 12, Y + 12 + num2, num, 12, 0);
		Engine.m_Edge[7].Draw(X + 12 + num, Y + 12 + num2, 0);
		Renderer.SetTexture(null);
		Renderer.PushAlpha(0.4f);
		Renderer.SolidRect(0, X + 12, Y + 12, num, num2);
		Renderer.PopAlpha();
	}

	private void Entry_OnClick(Gump Sender)
	{
		int entryID = (int)Sender.GetTag("EntryID");
		Network.Send(new PPopupResponse(this.m_Owner, entryID));
		Gumps.Destroy(this);
	}

	private GContextMenu(object owner, PopupEntry[] list)
		: base(100, 100)
	{
		this.m_Owner = owner;
		base.m_GUID = "MobilePopup";
		int num = 0;
		int num2 = 0;
		int num3 = list.Length;
		IFont uniFont = Engine.GetUniFont(3);
		IHue bright = Hues.Bright;
		IHue focusHue = Hues.Load(53);
		IHue hue = Hues.Default;
		OnClick onClick = Entry_OnClick;
		for (int i = 0; i < num3; i++)
		{
			PopupEntry popupEntry = list[i];
			GLabel gLabel = null;
			if (popupEntry.Flags == 1)
			{
				gLabel = new GLabel(popupEntry.Text, uniFont, hue, 7, 7 + num2);
			}
			else
			{
				gLabel = new GTextButton(popupEntry.Text, uniFont, bright, focusHue, 7, 7 + num2, onClick);
				gLabel.SetTag("EntryID", popupEntry.EntryID);
			}
			gLabel.X -= gLabel.Image.xMin;
			gLabel.Y -= gLabel.Image.yMin;
			num2 += gLabel.Image.yMax - gLabel.Image.yMin + 4;
			if (gLabel.Image.xMax - gLabel.Image.xMin + 1 > num)
			{
				num = gLabel.Image.xMax - gLabel.Image.xMin + 1;
			}
			base.m_Children.Add(gLabel);
		}
		num2 -= 3;
		this.m_Width = num + 14;
		this.m_Height = num2 + 14;
	}
}
