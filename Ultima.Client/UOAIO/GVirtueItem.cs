using System.Drawing;
using System.Windows.Forms;

namespace UOAIO;

public class GVirtueItem : GServerImage
{
	private static int[] m_Table;

	private GAlphaBackground m_Title;

	protected internal override bool HitTest(int x, int y)
	{
		if (base.m_Invalidated)
		{
			base.Refresh();
		}
		return base.m_Draw;
	}

	protected internal override void OnMouseEnter(int x, int y, MouseButtons mb)
	{
		if (this.m_Title != null)
		{
			this.m_Title.Visible = true;
		}
	}

	protected internal override void OnDoubleClick(int x, int y)
	{
		Network.Send(new PVirtueItemTrigger(base.m_Owner, base.m_GumpID));
	}

	protected internal override void OnMouseLeave()
	{
		if (this.m_Title != null)
		{
			this.m_Title.Visible = false;
		}
	}

	public GVirtueItem(GServerGump owner, int x, int y, int gumpID, IHue hue)
		: base(owner, x, y, gumpID, hue)
	{
		base.m_QuickDrag = false;
		int num = hue.HueID() - 1;
		int num2 = -1;
		int num3 = 0;
		for (int i = 0; i < GVirtueItem.m_Table.Length; i += 4)
		{
			if (GVirtueItem.m_Table[i] != gumpID)
			{
				continue;
			}
			num2 = i / 4;
			int num4 = 1;
			while (num4 < 4)
			{
				if (GVirtueItem.m_Table[i + num4] != num)
				{
					num4++;
					continue;
				}
				goto IL_0058;
			}
			continue;
			IL_0058:
			num3 = num4;
			break;
		}
		if (num2 >= 0)
		{
			this.m_Title = new GAlphaBackground(30 - x, 40 - y, 0, 0);
			GLabel gLabel = new GLabel(Localization.GetString(1051000 + num3 * 8 + num2), Engine.GetUniFont(0), hue, 3, 3);
			this.m_Title.Children.Add(gLabel);
			gLabel.X -= gLabel.Image.xMin;
			gLabel.Y -= gLabel.Image.yMin;
			this.m_Title.Width = gLabel.Image.xMax - gLabel.Image.xMin + 7;
			this.m_Title.Height = gLabel.Image.yMax - gLabel.Image.yMin + 7;
			Size size = Engine.m_Gumps.Measure(104);
			this.m_Title.X += (size.Width - this.m_Title.Width) / 2;
			this.m_Title.Y += (size.Height - this.m_Title.Height) / 2;
			this.m_Title.Visible = false;
			base.m_Children.Add(this.m_Title);
		}
	}

	static GVirtueItem()
	{
		GVirtueItem.m_Table = new int[32]
		{
			108, 1153, 2403, 2405, 110, 1546, 1551, 42, 105, 2212,
			2215, 52, 111, 2405, 2301, 1152, 112, 234, 2117, 32,
			107, 17, 617, 317, 109, 2209, 2211, 66, 106, 1347,
			1351, 97
		};
	}
}
