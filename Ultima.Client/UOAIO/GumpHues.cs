namespace UOAIO;

public class GumpHues
{
	private static IHue[] m_Hues;

	public static IHue Info => GumpHues.GetHue(GumpColors.Info, 0);

	public static IHue Menu => GumpHues.GetHue(GumpColors.Menu, 1);

	public static IHue Window => GumpHues.GetHue(GumpColors.Window, 2);

	public static IHue Control => GumpHues.GetHue(GumpColors.Control, 3);

	public static IHue Desktop => GumpHues.GetHue(GumpColors.Desktop, 4);

	public static IHue GrayText => GumpHues.GetHue(GumpColors.GrayText, 5);

	public static IHue HotTrack => GumpHues.GetHue(GumpColors.HotTrack, 6);

	public static IHue InfoText => GumpHues.GetHue(GumpColors.InfoText, 7);

	public static IHue MenuText => GumpHues.GetHue(GumpColors.MenuText, 8);

	public static IHue Highlight => GumpHues.GetHue(GumpColors.Highlight, 9);

	public static IHue ScrollBar => GumpHues.GetHue(GumpColors.ScrollBar, 10);

	public static IHue WindowText => GumpHues.GetHue(GumpColors.WindowText, 11);

	public static IHue ControlDark => GumpHues.GetHue(GumpColors.ControlDark, 12);

	public static IHue ControlText => GumpHues.GetHue(GumpColors.ControlText, 13);

	public static IHue WindowFrame => GumpHues.GetHue(GumpColors.WindowFrame, 14);

	public static IHue ActiveBorder => GumpHues.GetHue(GumpColors.ActiveBorder, 15);

	public static IHue AppWorkspace => GumpHues.GetHue(GumpColors.AppWorkspace, 16);

	public static IHue ControlLight => GumpHues.GetHue(GumpColors.ControlLight, 17);

	public static IHue ActiveCaption => GumpHues.GetHue(GumpColors.ActiveCaption, 18);

	public static IHue HighlightText => GumpHues.GetHue(GumpColors.HighlightText, 19);

	public static IHue InactiveBorder => GumpHues.GetHue(GumpColors.InactiveBorder, 20);

	public static IHue ControlDarkDark => GumpHues.GetHue(GumpColors.ControlDarkDark, 21);

	public static IHue InactiveCaption => GumpHues.GetHue(GumpColors.InactiveCaption, 22);

	public static IHue ActiveCaptionText => GumpHues.GetHue(GumpColors.ActiveCaptionText, 23);

	public static IHue ControlLightLight => GumpHues.GetHue(GumpColors.ControlLightLight, 24);

	public static IHue InactiveCaptionText => GumpHues.GetHue(GumpColors.InactiveCaptionText, 25);

	public static IHue ControlAlternate => GumpHues.GetHue(GumpColors.ControlAlternate, 26);

	public static void Invalidate()
	{
		for (int i = 0; i < GumpHues.m_Hues.Length; i++)
		{
			GumpHues.m_Hues[i] = null;
		}
	}

	public static IHue GetHue(int c, int idx)
	{
		if (GumpHues.m_Hues[idx] == null)
		{
			GumpHues.m_Hues[idx] = new Hues.ColorFillHue(c);
		}
		return GumpHues.m_Hues[idx];
	}

	static GumpHues()
	{
		GumpHues.m_Hues = new IHue[27];
	}
}
