using System.Drawing;

namespace UOAIO;

public class GumpPaint
{
	private static Clipper m_Clipper;

	public static Clipper Clipper
	{
		get
		{
			return GumpPaint.m_Clipper;
		}
		set
		{
			GumpPaint.m_Clipper = value;
		}
	}

	public static void Invalidate()
	{
	}

	public static void DrawRect(Color c, int x, int y, int w, int h)
	{
		if (GumpPaint.m_Clipper == null)
		{
			Renderer.SolidRect(c.ToArgb() & 0xFFFFFF, x, y, w, h);
		}
		else
		{
			Renderer.SolidRect(c.ToArgb() & 0xFFFFFF, x, y, w, h, GumpPaint.m_Clipper);
		}
	}

	public static void DrawRect(int c, int x, int y, int w, int h)
	{
		if (GumpPaint.m_Clipper == null)
		{
			Renderer.SolidRect(c, x, y, w, h);
		}
		else
		{
			Renderer.SolidRect(c, x, y, w, h, GumpPaint.m_Clipper);
		}
	}

	public static void DrawFlat(int x, int y, int w, int h)
	{
		GumpPaint.DrawRect(SystemColors.ControlDark, x, y, w, h);
		GumpPaint.DrawRect(SystemColors.Control, x + 1, y + 1, w - 2, h - 2);
	}

	public static void DrawFlat(int x, int y, int w, int h, int border, int fill)
	{
		GumpPaint.DrawRect(border, x, y, w, h);
		GumpPaint.DrawRect(fill, x + 1, y + 1, w - 2, h - 2);
	}

	public static void DrawRaised3D(int x, int y, int w, int h)
	{
		GumpPaint.DrawRect(SystemColors.ControlDarkDark, x, y, w, h);
		GumpPaint.DrawRect(SystemColors.ControlLight, x, y, w - 1, h - 1);
		GumpPaint.DrawRect(SystemColors.ControlDark, x + 1, y + 1, w - 2, h - 2);
		GumpPaint.DrawRect(SystemColors.ControlLightLight, x + 1, y + 1, w - 3, h - 3);
		GumpPaint.DrawRect(SystemColors.Control, x + 2, y + 2, w - 4, h - 4);
	}

	public static void DrawRaised3D(int x, int y, int w, int h, int fill)
	{
		GumpPaint.DrawRect(SystemColors.ControlDarkDark, x, y, w, h);
		GumpPaint.DrawRect(SystemColors.ControlLight, x, y, w - 1, h - 1);
		GumpPaint.DrawRect(SystemColors.ControlDark, x + 1, y + 1, w - 2, h - 2);
		GumpPaint.DrawRect(SystemColors.ControlLightLight, x + 1, y + 1, w - 3, h - 3);
		GumpPaint.DrawRect(fill, x + 2, y + 2, w - 4, h - 4);
	}

	public static void DrawSunken3D(int x, int y, int w, int h, int fill)
	{
		GumpPaint.DrawRect(SystemColors.ControlLightLight, x, y, w, h);
		GumpPaint.DrawRect(SystemColors.ControlDark, x, y, w - 1, h - 1);
		GumpPaint.DrawRect(SystemColors.ControlLight, x + 1, y + 1, w - 2, h - 2);
		GumpPaint.DrawRect(SystemColors.ControlDarkDark, x + 1, y + 1, w - 3, h - 3);
		GumpPaint.DrawRect(fill, x + 2, y + 2, w - 4, h - 4);
	}

	public static void DrawSunken3D(int x, int y, int w, int h)
	{
		GumpPaint.DrawRect(SystemColors.ControlLightLight, x, y, w, h);
		GumpPaint.DrawRect(SystemColors.ControlDark, x, y, w - 1, h - 1);
		GumpPaint.DrawRect(SystemColors.ControlLight, x + 1, y + 1, w - 2, h - 2);
		GumpPaint.DrawRect(SystemColors.ControlDarkDark, x + 1, y + 1, w - 3, h - 3);
		GumpPaint.DrawRect(SystemColors.Control, x + 2, y + 2, w - 4, h - 4);
	}

	public static Color Blend(Color c1, Color c2, float f)
	{
		return GumpPaint.Blend(c1, c2, (int)(f * 255f));
	}

	public static Color Blend(Color c1, Color c2, int p)
	{
		return Color.FromArgb((c1.R * p + c2.R * (255 - p)) / 255, (c1.G * p + c2.G * (255 - p)) / 255, (c1.B * p + c2.B * (255 - p)) / 255);
	}

	public static int Blend(int c1, int c2, float f)
	{
		return GumpPaint.Blend(Color.FromArgb(c1), Color.FromArgb(c2), f).ToArgb() & 0xFFFFFF;
	}

	public static int Blend(int c1, int c2, int p)
	{
		return GumpPaint.Blend(Color.FromArgb(c1), Color.FromArgb(c2), p).ToArgb() & 0xFFFFFF;
	}
}
