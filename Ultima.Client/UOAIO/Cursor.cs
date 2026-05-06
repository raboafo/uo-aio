using System.Windows.Forms;
using UOAIO.Targeting;

namespace UOAIO;

public class Cursor
{
	private static CursorEntry[,] m_Cursors;

	private static bool m_Visible;

	private static bool m_Gold;

	private static bool m_Hourglass;

	public static bool Hourglass
	{
		get
		{
			return Cursor.m_Hourglass;
		}
		set
		{
			Cursor.m_Hourglass = value;
		}
	}

	public static bool Gold
	{
		get
		{
			return Cursor.m_Gold;
		}
		set
		{
			Cursor.m_Gold = value;
		}
	}

	public static bool Visible
	{
		get
		{
			return Cursor.m_Visible;
		}
		set
		{
			Cursor.m_Visible = value;
		}
	}

	public static int Height => Cursor.GetCursor()?.m_Image.Height ?? 0;

	public static int Width => Cursor.GetCursor()?.m_Image.Width ?? 0;

	public static void MoveTo(Gump who)
	{
		Point point = who.PointToScreen(new Point(who.Width / 2, who.Height / 2));
		System.Windows.Forms.Cursor.Position = Engine.m_Display.PointToScreen(point);
		Gumps.Invalidate();
	}

	public static CursorEntry GetCursor()
	{
		if (!Cursor.m_Visible)
		{
			return null;
		}
		int num = 7;
		int num2 = 0;
		if (TargetManager.IsActive)
		{
			num = 12;
		}
		else if (Cursor.m_Hourglass)
		{
			num = 13;
		}
		else if (Gumps.Drag != null && Gumps.Drag.m_DragCursor)
		{
			num = 8;
		}
		else if (Gumps.LastOver != null && Gumps.LastOver.m_OverridesCursor)
		{
			num = Gumps.LastOver.m_OverCursor;
		}
		else if (GObjectProperties.Instance != null)
		{
			num = 9;
		}
		else if (DesignContext.Current != null && DesignContext.Current.Entry != null)
		{
			num = 12;
		}
		else if (Engine.m_Ingame)
		{
			num = (int)Engine.pointingDir;
		}
		if (Engine.m_Ingame)
		{
			Mobile player = World.Player;
			if (player != null)
			{
				if (player.Flags[MobileFlag.Warmode])
				{
					num2 = 1;
				}
				else if (Cursor.m_Gold)
				{
					num2 = 2;
				}
			}
			else if (Cursor.m_Gold)
			{
				num2 = 2;
			}
		}
		CursorEntry cursorEntry = Cursor.m_Cursors[num, num2];
		if (cursorEntry == null)
		{
			cursorEntry = (Cursor.m_Cursors[num, num2] = Cursor.LoadCursor(num, num2));
		}
		return cursorEntry;
	}

	private unsafe static CursorEntry LoadCursor(int idCursor, int idType)
	{
		IHue hue;
		int itemId;
		switch (idType)
		{
		case 0:
			hue = Hues.Default;
			itemId = 8298 + idCursor;
			break;
		case 1:
			hue = Hues.Default;
			itemId = 8275 + idCursor;
			break;
		case 2:
			hue = Hues.Load(35181);
			itemId = 8298 + idCursor;
			break;
		default:
			return null;
		}
		Texture item = hue.GetItem(itemId);
		if (item == null || item.IsEmpty())
		{
			return new CursorEntry(0, 0, 0, 0, Texture.Empty);
		}
		if (item.m_Factory != null)
		{
			item.m_Factory.Remove(item);
			item.m_Factory = null;
			item.m_FactoryArgs = null;
		}
		int xOffset = 0;
		int yOffset = 0;
		if (idType < 2)
		{
			LockData lockData = item.Lock(LockFlags.ReadWrite);
			int width = lockData.Width;
			int height = lockData.Height;
			int num = lockData.Pitch >> 1;
			short* pvSrc = (short*)lockData.pvSrc;
			short* pvSrc2 = (short*)lockData.pvSrc;
			pvSrc2 += (height - 1) * num;
			for (int i = 0; i < width; i++)
			{
				if ((*pvSrc & 0x7FFF) == 992)
				{
					xOffset = i;
				}
				*(pvSrc++) = 0;
				*(pvSrc2++) = 0;
			}
			pvSrc = (short*)lockData.pvSrc;
			pvSrc2 = (short*)lockData.pvSrc;
			pvSrc2 += width - 1;
			for (int j = 0; j < height; j++)
			{
				if ((*pvSrc & 0x7FFF) == 992)
				{
					yOffset = j;
				}
				*pvSrc = 0;
				*pvSrc2 = 0;
				pvSrc += num;
				pvSrc2 += num;
			}
			item.Unlock();
		}
		else
		{
			CursorEntry cursorEntry = Cursor.m_Cursors[idCursor, 1];
			if (cursorEntry == null)
			{
				cursorEntry = Cursor.m_Cursors[idCursor, 0];
				if (cursorEntry == null)
				{
					cursorEntry = (Cursor.m_Cursors[idCursor, 1] = Cursor.LoadCursor(idCursor, 1));
				}
			}
			xOffset = cursorEntry.m_xOffset;
			yOffset = cursorEntry.m_yOffset;
			LockData lockData2 = item.Lock(LockFlags.ReadWrite);
			int width2 = lockData2.Width;
			int height2 = lockData2.Height;
			int num2 = lockData2.Pitch >> 1;
			short* pvSrc3 = (short*)lockData2.pvSrc;
			short* pvSrc4 = (short*)lockData2.pvSrc;
			pvSrc4 += (height2 - 1) * num2;
			for (int k = 0; k < width2; k++)
			{
				*(pvSrc3++) = 0;
				*(pvSrc4++) = 0;
			}
			pvSrc3 = (short*)lockData2.pvSrc;
			pvSrc4 = (short*)lockData2.pvSrc;
			pvSrc4 += width2 - 1;
			for (int l = 0; l < height2; l++)
			{
				*pvSrc3 = 0;
				*pvSrc4 = 0;
				pvSrc3 += num2;
				pvSrc4 += num2;
			}
			item.Unlock();
		}
		return new CursorEntry(idCursor, idType, xOffset, yOffset, item);
	}

	public static void Draw()
	{
		Cursor.GetCursor()?.Draw(Engine.m_xMouse, Engine.m_yMouse);
	}

	public static void Dispose()
	{
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				if (Cursor.m_Cursors[i, j] != null)
				{
					Cursor.m_Cursors[i, j].m_Image.Dispose();
					Cursor.m_Cursors[i, j].m_Image = null;
				}
			}
		}
		Cursor.m_Cursors = null;
	}

	static Cursor()
	{
		Cursor.m_Cursors = new CursorEntry[16, 3];
		Cursor.m_Visible = true;
	}
}
