using System.Drawing;
using Microsoft.Win32;

namespace UOAIO;

public class GumpColors
{
	private static int m_ControlAlternate;

	private static int m_ActiveCaptionGradient;

	private static int m_InactiveCaptionGradient;

	public static int Info => SystemColors.Info.ToArgb() & 0xFFFFFF;

	public static int Menu => SystemColors.Menu.ToArgb() & 0xFFFFFF;

	public static int Window => SystemColors.Window.ToArgb() & 0xFFFFFF;

	public static int Control => SystemColors.Control.ToArgb() & 0xFFFFFF;

	public static int Desktop => SystemColors.Desktop.ToArgb() & 0xFFFFFF;

	public static int GrayText => SystemColors.GrayText.ToArgb() & 0xFFFFFF;

	public static int HotTrack => SystemColors.HotTrack.ToArgb() & 0xFFFFFF;

	public static int InfoText => SystemColors.InfoText.ToArgb() & 0xFFFFFF;

	public static int MenuText => SystemColors.MenuText.ToArgb() & 0xFFFFFF;

	public static int Highlight => SystemColors.Highlight.ToArgb() & 0xFFFFFF;

	public static int ScrollBar => SystemColors.ScrollBar.ToArgb() & 0xFFFFFF;

	public static int WindowText => SystemColors.WindowText.ToArgb() & 0xFFFFFF;

	public static int ControlDark => SystemColors.ControlDark.ToArgb() & 0xFFFFFF;

	public static int ControlText => SystemColors.ControlText.ToArgb() & 0xFFFFFF;

	public static int WindowFrame => SystemColors.WindowFrame.ToArgb() & 0xFFFFFF;

	public static int ActiveBorder => SystemColors.ActiveBorder.ToArgb() & 0xFFFFFF;

	public static int AppWorkspace => SystemColors.AppWorkspace.ToArgb() & 0xFFFFFF;

	public static int ControlLight => SystemColors.ControlLight.ToArgb() & 0xFFFFFF;

	public static int ActiveCaption => SystemColors.ActiveCaption.ToArgb() & 0xFFFFFF;

	public static int HighlightText => SystemColors.HighlightText.ToArgb() & 0xFFFFFF;

	public static int InactiveBorder => SystemColors.InactiveBorder.ToArgb() & 0xFFFFFF;

	public static int ControlDarkDark => SystemColors.ControlDarkDark.ToArgb() & 0xFFFFFF;

	public static int InactiveCaption => SystemColors.InactiveCaption.ToArgb() & 0xFFFFFF;

	public static int ActiveCaptionText => SystemColors.ActiveCaptionText.ToArgb() & 0xFFFFFF;

	public static int ControlLightLight => SystemColors.ControlLightLight.ToArgb() & 0xFFFFFF;

	public static int InactiveCaptionText => SystemColors.InactiveCaptionText.ToArgb() & 0xFFFFFF;

	public static int ControlAlternate
	{
		get
		{
			if (GumpColors.m_ControlAlternate >= 0)
			{
				return GumpColors.m_ControlAlternate;
			}
			return GumpColors.m_ControlAlternate = GumpColors.ReadRegistryColor("ButtonAlternateFace").ToArgb() & 0xFFFFFF;
		}
	}

	public static int ActiveCaptionGradient
	{
		get
		{
			if (GumpColors.m_ActiveCaptionGradient >= 0)
			{
				return GumpColors.m_ActiveCaptionGradient;
			}
			return GumpColors.m_ActiveCaptionGradient = GumpColors.ReadRegistryColor("GradientActiveTitle").ToArgb() & 0xFFFFFF;
		}
	}

	public static int InactiveCaptionGradient
	{
		get
		{
			if (GumpColors.m_InactiveCaptionGradient >= 0)
			{
				return GumpColors.m_InactiveCaptionGradient;
			}
			return GumpColors.m_InactiveCaptionGradient = GumpColors.ReadRegistryColor("GradientInactiveTitle").ToArgb() & 0xFFFFFF;
		}
	}

	public static void Invalidate()
	{
		GumpColors.m_ControlAlternate = -1;
		GumpColors.m_ActiveCaptionGradient = -1;
		GumpColors.m_InactiveCaptionGradient = -1;
	}

	private static Color ReadRegistryColor(string name)
	{
		try
		{
			using RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Control Panel\\Colors", writable: false);
			string text = registryKey.GetValue(name) as string;
			string[] array = text.Split(' ');
			return Color.FromArgb(int.Parse(array[0]), int.Parse(array[1]), int.Parse(array[2]));
		}
		catch
		{
		}
		return Color.White;
	}

	static GumpColors()
	{
		GumpColors.m_ControlAlternate = -1;
		GumpColors.m_ActiveCaptionGradient = -1;
		GumpColors.m_InactiveCaptionGradient = -1;
	}
}
