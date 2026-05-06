using System;
using System.Collections;

namespace UOAIO;

public class GHyperLink : GTextButton
{
	private string m_Url;

	private static IHue RegularHue;

	private static IHue VisitedHue;

	private static Hashtable m_Visited;

	public GHyperLink(string url, string text, IFont font, int x, int y)
		: base(text, font, GHyperLink.m_Visited.Contains(url) ? GHyperLink.VisitedHue : GHyperLink.RegularHue, GHyperLink.m_Visited.Contains(url) ? GHyperLink.VisitedHue : GHyperLink.RegularHue, x, y, null)
	{
		base.Underline = true;
		this.m_Url = url;
		base.OnClick = Button_OnClick;
	}

	private void Button_OnClick(Gump g)
	{
		Engine.OpenBrowser(this.m_Url);
		GHyperLink.m_Visited[this.m_Url] = true;
		IHue defaultHue = (base.FocusHue = GHyperLink.VisitedHue);
		base.DefaultHue = defaultHue;
	}

	static GHyperLink()
	{
		GHyperLink.RegularHue = new Hues.ColorFillHue(255);
		GHyperLink.VisitedHue = new Hues.ColorFillHue(16711680);
		GHyperLink.m_Visited = new Hashtable(StringComparer.OrdinalIgnoreCase);
	}
}
