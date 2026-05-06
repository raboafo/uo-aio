using System;
using System.Collections.Generic;
using System.Drawing;

namespace UOAIO;

public class GServerGump : Gump
{
	private class LocationCacheEntry
	{
		public int m_DialogID;

		public int m_xOffset;

		public int m_yOffset;

		public LocationCacheEntry(int dialogID)
		{
			this.m_DialogID = dialogID;
		}
	}

	private static Dictionary<int, LocationCacheEntry> m_LocationCache;

	private bool m_CanClose;

	private bool m_CanMove;

	private Dictionary<int, GumpList> m_Pages;

	private int m_Page;

	private int m_Serial;

	private int m_DialogID;

	private LayoutEntry[] m_Entries;

	private string[] m_Text;

	private List<Gump> m_AlphaRegions = new List<Gump>();

	private static char[] m_FirstChars;

	private static char[] m_SecondChars;

	public bool CanClose => this.m_CanClose;

	public bool CanMove => this.m_CanMove;

	public int Serial => this.m_Serial;

	public int DialogID => this.m_DialogID;

	public int Page
	{
		get
		{
			return this.m_Page;
		}
		set
		{
			if (this.m_Page == value)
			{
				return;
			}
			this.m_Page = value;
			base.m_Children.Set(this.Pages(0));
			if (this.m_Page != 0)
			{
				base.m_Children.Add(this.Pages(this.m_Page));
			}
			this.m_AlphaRegions.Clear();
			for (int i = 0; i < base.m_Children.Count; i++)
			{
				if (base.m_Children[i] is GTransparentRegion)
				{
					this.m_AlphaRegions.Add(base.m_Children[i]);
				}
			}
		}
	}

	public static void GetCachedLocation(int dialogID, ref int x, ref int y)
	{
		LocationCacheEntry value = null;
		if (GServerGump.m_LocationCache.TryGetValue(dialogID, out value))
		{
			x = value.m_xOffset;
			y = value.m_yOffset;
			GServerGump.m_LocationCache.Remove(dialogID);
		}
	}

	public static void SetCachedLocation(int dialogID, int x, int y)
	{
		LocationCacheEntry value = null;
		if (!GServerGump.m_LocationCache.TryGetValue(dialogID, out value))
		{
			value = (GServerGump.m_LocationCache[dialogID] = new LocationCacheEntry(dialogID));
		}
		value.m_xOffset = x;
		value.m_yOffset = y;
	}

	public static void ClearCachedLocation(int dialogID)
	{
		GServerGump.m_LocationCache.Remove(dialogID);
	}

	protected internal override void Render(int X, int Y)
	{
		if (this.m_AlphaRegions.Count == 0)
		{
			base.Render(X, Y);
		}
		else
		{
			if (!base.m_Visible)
			{
				return;
			}
			int num = X + this.X;
			int num2 = Y + this.Y;
			this.Draw(num, num2);
			Gump[] array = base.m_Children.ToArray();
			RectangleList rectangleList = new RectangleList();
			RectangleList rectangleList2 = new RectangleList();
			int num3 = 0;
			for (int i = 0; i < array.Length; i++)
			{
				Gump gump = array[i];
				if (gump is GTransparentRegion)
				{
					num3++;
					continue;
				}
				if (num3 >= this.m_AlphaRegions.Count)
				{
					gump.Render(num, num2);
					continue;
				}
				Rectangle rectangle = new Rectangle(gump.X, gump.Y, gump.Width, gump.Height);
				rectangleList.Add(rectangle);
				for (int j = num3; j < this.m_AlphaRegions.Count; j++)
				{
					Gump gump2 = this.m_AlphaRegions[j];
					if (gump2 is GTransparentRegion)
					{
						Rectangle a = new Rectangle(gump2.X, gump2.Y, gump2.Width, gump2.Height);
						Rectangle rect = Rectangle.Intersect(a, rectangle);
						rectangleList.Remove(rect);
						rectangleList2.Add(rect);
					}
				}
				if (rectangleList2.Count > 0)
				{
					for (int k = i + 1; k < array.Length; k++)
					{
						Gump gump3 = array[k];
						if (gump3 is GServerBackground)
						{
							GServerBackground gServerBackground = (GServerBackground)gump3;
							Rectangle rect2 = new Rectangle(gServerBackground.X + gServerBackground.OffsetX, gServerBackground.Y + gServerBackground.OffsetY, gServerBackground.UseWidth, gServerBackground.UseHeight);
							rectangleList.Remove(rect2);
							rectangleList2.Remove(rect2);
						}
						else if (gump3 == this.m_AlphaRegions[this.m_AlphaRegions.Count - 1])
						{
							break;
						}
					}
					if (rectangleList2.Count == 1 && rectangleList.Count == 0)
					{
						Renderer.PushAlpha(0.5f);
						gump.Render(num, num2);
						Renderer.PopAlpha();
					}
					else
					{
						for (int l = 0; l < rectangleList.Count; l++)
						{
							Rectangle rectangle2 = rectangleList[l];
							if (Renderer.SetViewport(num + rectangle2.X, num2 + rectangle2.Y, rectangle2.Width, rectangle2.Height))
							{
								gump.Render(num, num2);
							}
						}
						for (int m = 0; m < rectangleList2.Count; m++)
						{
							Rectangle rectangle3 = rectangleList2[m];
							if (Renderer.SetViewport(num + rectangle3.X, num2 + rectangle3.Y, rectangle3.Width, rectangle3.Height))
							{
								Renderer.PushAlpha(0.5f);
								gump.Render(num, num2);
								Renderer.PopAlpha();
							}
						}
						if (rectangleList.Count > 0 || rectangleList2.Count > 0)
						{
							Renderer.SetViewport(0, 0, Engine.ScreenWidth, Engine.ScreenHeight);
						}
					}
					rectangleList.Clear();
					rectangleList2.Clear();
				}
				else
				{
					gump.Render(num, num2);
				}
			}
		}
	}

	public string GetValue(int index)
	{
		if (index < 0 || index >= this.m_Entries.Length)
		{
			return null;
		}
		LayoutEntry layoutEntry = this.m_Entries[index];
		switch (layoutEntry.Name)
		{
		case "text":
			return this.m_Text[layoutEntry[3]];
		case "croppedtext":
			return this.m_Text[layoutEntry[5]];
		case "xmfhtmlgump":
		case "xmfhtmlgumpcolor":
			return "#" + layoutEntry[4];
		case "htmlgump":
			return this.m_Text[layoutEntry[4]];
		default:
		{
			string text = layoutEntry.Name;
			for (int i = 0; i < layoutEntry.Parameters.Length; i++)
			{
				text = text + " " + layoutEntry.Parameters[i];
			}
			return text;
		}
		}
	}

	public GumpList Pages(int page)
	{
		GumpList value = null;
		if (!this.m_Pages.TryGetValue(page, out value))
		{
			value = (this.m_Pages[page] = new GumpList(this));
		}
		return value;
	}

	public static Point3D ReverseLookup(int f, int xLong, int yLat, int xMins, int yMins, bool xEast, bool ySouth)
	{
		if (!GServerGump.ComputeMapDetails(f, 0, 0, out var xCenter, out var yCenter, out var xWidth, out var yHeight))
		{
			return new Point3D(0, 0, 0);
		}
		double num = (double)xLong + (double)xMins / 60.0;
		double num2 = (double)yLat + (double)yMins / 60.0;
		if (!xEast)
		{
			num = 360.0 - num;
		}
		if (!ySouth)
		{
			num2 = 360.0 - num2;
		}
		int num3 = xCenter + (int)(num * (double)xWidth / 360.0);
		int num4 = yCenter + (int)(num2 * (double)yHeight / 360.0);
		if (num3 < 0)
		{
			num3 += xWidth;
		}
		else if (num3 >= xWidth)
		{
			num3 -= xWidth;
		}
		if (num4 < 0)
		{
			num4 += yHeight;
		}
		else if (num4 >= yHeight)
		{
			num4 -= yHeight;
		}
		int z = 0;
		return new Point3D(num3, num4, z);
	}

	public static bool ComputeMapDetails(int facet, int x, int y, out int xCenter, out int yCenter, out int xWidth, out int yHeight)
	{
		xWidth = 5120;
		yHeight = 4096;
		if (facet < 2)
		{
			if (x >= 0 && y >= 0 && x < 5120 && y < 4096)
			{
				xCenter = 1323;
				yCenter = 1624;
			}
			else
			{
				if (x < 5120 || y < 2304 || x >= 6144 || y >= 4096)
				{
					xCenter = 0;
					yCenter = 0;
					return false;
				}
				xCenter = 5936;
				yCenter = 3112;
			}
		}
		else
		{
			xCenter = 1323;
			yCenter = 1624;
		}
		return true;
	}

	private bool ParseCoord(string format, out int degrees, out int minutes, out bool direction)
	{
		try
		{
			int num = 0;
			int num2 = format.IndexOfAny(GServerGump.m_FirstChars, num);
			degrees = int.Parse(format.Substring(num, num2 - num));
			num = num2 + 1;
			num2 = format.IndexOfAny(GServerGump.m_SecondChars, num);
			minutes = int.Parse(format.Substring(num, num2 - num));
			char c = format[format.Length - 1];
			direction = c == 'S' || c == 'E';
			return true;
		}
		catch
		{
			degrees = 0;
			minutes = 0;
			direction = false;
			Debug.Trace("failed to parse -> {0}", format);
			return false;
		}
	}

	public GServerGump(int serial, int dialogID, int x, int y, string layout, string[] text)
		: base(x, y)
	{
		this.m_Serial = serial;
		this.m_DialogID = dialogID;
		this.m_CanClose = true;
		this.m_CanMove = true;
		base.m_NonRestrictivePicking = true;
		this.m_Pages = new Dictionary<int, GumpList>();
		this.m_Page = -1;
		LayoutEntry[] array = this.ParseLayout(layout);
		this.ProcessLayout(array, text);
		this.m_Text = text;
		this.m_Entries = array;
	}

	private LayoutEntry[] ParseLayout(string layout)
	{
		using ScratchList<LayoutEntry> scratchList = new ScratchList<LayoutEntry>();
		List<LayoutEntry> value = scratchList.Value;
		int num = 0;
		int num2;
		while ((num2 = layout.IndexOf('{', num)) >= 0)
		{
			num2++;
			num = layout.IndexOf('}', num2);
			value.Add(new LayoutEntry(layout.Substring(num2, num - num2).Trim()));
		}
		return value.ToArray();
	}

	private void ProcessLayout(LayoutEntry[] list, string[] text)
	{
		int num = 0;
		int num2 = 0;
		bool flag = false;
		int num3 = 0;
		foreach (LayoutEntry layoutEntry in list)
		{
			switch (layoutEntry.Name)
			{
			case "checkertrans":
				this.Pages(num).Add(new GTransparentRegion(this, layoutEntry[0], layoutEntry[1], layoutEntry[2], layoutEntry[3]));
				break;
			case "page":
				num = layoutEntry[0];
				if (num == 0)
				{
					flag = false;
					num2 = 0;
				}
				else if (!flag || num < num2)
				{
					num2 = num;
					flag = true;
				}
				break;
			case "group":
				num3 = layoutEntry[0];
				break;
			case "noclose":
				this.m_CanClose = false;
				break;
			case "nomove":
				this.m_CanMove = false;
				break;
			case "resizepic":
				this.Pages(num).Add(new GServerBackground(this, layoutEntry[0], layoutEntry[1], layoutEntry[3], layoutEntry[4], layoutEntry[2] + 4, border: true));
				break;
			case "gumppictiled":
				this.Pages(num).Add(new GServerBackground(this, layoutEntry[0], layoutEntry[1], layoutEntry[2], layoutEntry[3], layoutEntry[4], border: false));
				break;
			case "gumppic":
			{
				string attribute = layoutEntry.GetAttribute("hue");
				IHue hue;
				if (attribute != null)
				{
					try
					{
						hue = Hues.Load(Convert.ToInt32(attribute));
					}
					catch
					{
						hue = Hues.Default;
					}
				}
				else
				{
					hue = Hues.Default;
				}
				string attribute2 = layoutEntry.GetAttribute("class");
				if (attribute2 == "VirtueGumpItem")
				{
					this.Pages(num).Add(new GVirtueItem(this, layoutEntry[0], layoutEntry[1], layoutEntry[2], hue));
				}
				else
				{
					this.Pages(num).Add(new GServerImage(this, layoutEntry[0], layoutEntry[1], layoutEntry[2], hue));
				}
				break;
			}
			case "textentry":
				this.Pages(num).Add(new GServerTextBox(text[layoutEntry[6]], layoutEntry));
				break;
			case "tilepic":
				this.Pages(num).Add(new GItemArt(layoutEntry[0], layoutEntry[1], layoutEntry[2]));
				break;
			case "tilepichue":
				this.Pages(num).Add(new GItemArt(layoutEntry[0], layoutEntry[1], layoutEntry[2], Hues.GetItemHue(layoutEntry[2], layoutEntry[3])));
				break;
			case "button":
				this.Pages(num).Add(new GServerButton(this, layoutEntry));
				break;
			case "radio":
				this.Pages(num).Add(new GServerRadio(this, layoutEntry, num3));
				break;
			case "checkbox":
				this.Pages(num).Add(new GServerCheckBox(this, layoutEntry));
				break;
			case "text":
			{
				int num6 = layoutEntry[2];
				this.Pages(num).Add(new GLabel(text[layoutEntry[3]], Engine.GetUniFont(1), Hues.Load(num6 + 1), layoutEntry[0] - 1, layoutEntry[1]));
				break;
			}
			case "croppedtext":
			{
				int num4 = layoutEntry[4];
				string text2 = text[layoutEntry[5]];
				int num5 = layoutEntry[2];
				IFont uniFont = Engine.GetUniFont(1);
				if (uniFont.GetStringWidth(text2) > num5)
				{
					while (text2.Length > 0 && uniFont.GetStringWidth(text2 + "...") > num5)
					{
						text2 = text2.Substring(0, text2.Length - 1);
					}
					text2 += "...";
				}
				GLabel gLabel = new GLabel(text2, uniFont, Hues.Load(num4 + 1), layoutEntry[0] - 1, layoutEntry[1]);
				gLabel.Scissor(0, 0, num5, layoutEntry[3]);
				this.Pages(num).Add(gLabel);
				break;
			}
			case "xmfhtmlgump":
				this.ProcessHtmlGump(num, layoutEntry[0], layoutEntry[1], layoutEntry[2], layoutEntry[3], layoutEntry[5] != 0, layoutEntry[6] != 0, (layoutEntry[5] == 0 && layoutEntry[6] != 0) ? 16777215 : 0, Localization.GetString(layoutEntry[4]), null);
				break;
			case "xmfhtmlgumpcolor":
				this.ProcessHtmlGump(num, layoutEntry[0], layoutEntry[1], layoutEntry[2], layoutEntry[3], layoutEntry[5] != 0, layoutEntry[6] != 0, Engine.C16232(layoutEntry[7]), Localization.GetString(layoutEntry[4]), null);
				break;
			case "htmlgump":
				this.ProcessHtmlGump(num, layoutEntry[0], layoutEntry[1], layoutEntry[2], layoutEntry[3], layoutEntry[5] != 0, layoutEntry[6] != 0, (layoutEntry[5] == 0 && layoutEntry[6] != 0) ? 16777215 : 0, text[layoutEntry[4]], null);
				break;
			case "xmfhtmltok":
				this.ProcessHtmlGump(num, layoutEntry[0], layoutEntry[1], layoutEntry[2], layoutEntry[3], layoutEntry[4] != 0, layoutEntry[5] != 0, Engine.C16232(layoutEntry[6]), Localization.GetString(layoutEntry[7]), layoutEntry[8].ToString());
				Console.WriteLine(layoutEntry[8]);
				break;
			default:
				Engine.AddTextMessage(layoutEntry.Name);
				break;
			case "nodispose":
			case "noresize":
				break;
			}
		}
		this.Page = ((num2 == 0) ? 1 : num2);
	}

	private string ParseGumpArgs(string text, string args)
	{
		if (text.Contains("~1_AMT~") && args != null)
		{
			Engine.AddTextMessage(args);
			return text.Replace("~1_AMT~", args);
		}
		return text;
	}

	private void OnScroll(double vNew, double vOld, Gump g)
	{
		int num = (int)vNew;
		Gump gump = (Gump)g.GetTag("toScroll");
		int num2 = (int)g.GetTag("yBase");
		int h = (int)g.GetTag("viewHeight");
		gump.Y = num2 - num;
		((GHtmlLabel)gump).Scissor(0, num, gump.Width, h);
	}

	private void ProcessHtmlGump(int thisPage, int x, int y, int width, int height, bool hasBack, bool hasScroll, int color, string text, string args)
	{
		UnicodeFont uniFont = Engine.GetUniFont(1);
		if (!hasScroll)
		{
			if (hasBack)
			{
				GServerBackground gServerBackground = new GServerBackground(this, x, y, width, height, 3004, border: true);
				GHtmlLabel gHtmlLabel = new GHtmlLabel(this.ParseGumpArgs(text, args), uniFont, color, gServerBackground.OffsetX - 2, gServerBackground.OffsetY - 1, gServerBackground.UseWidth);
				gHtmlLabel.Scissor(0, 0, gHtmlLabel.Width, gServerBackground.UseHeight);
				gServerBackground.Children.Add(gHtmlLabel);
				this.Pages(thisPage).Add(gServerBackground);
			}
			else
			{
				GHtmlLabel gHtmlLabel2 = new GHtmlLabel(this.ParseGumpArgs(text, args), uniFont, color, x - 2, y - 1, width);
				gHtmlLabel2.Scissor(0, 0, gHtmlLabel2.Width, height);
				this.Pages(thisPage).Add(gHtmlLabel2);
			}
			return;
		}
		width -= 15;
		GHtmlLabel gHtmlLabel3;
		int num;
		if (hasBack)
		{
			GServerBackground gServerBackground2 = new GServerBackground(this, x, y, width, height, 3004, border: true);
			gHtmlLabel3 = new GHtmlLabel(this.ParseGumpArgs(text, args), uniFont, color, gServerBackground2.OffsetX - 2, gServerBackground2.OffsetY - 1, gServerBackground2.UseWidth);
			gHtmlLabel3.Scissor(0, 0, gHtmlLabel3.Width, num = gServerBackground2.UseHeight);
			gServerBackground2.Children.Add(gHtmlLabel3);
			this.Pages(thisPage).Add(gServerBackground2);
		}
		else
		{
			gHtmlLabel3 = new GHtmlLabel(this.ParseGumpArgs(text, args), uniFont, color, x - 2, y - 1, width);
			gHtmlLabel3.Scissor(0, 0, gHtmlLabel3.Width, num = height);
			this.Pages(thisPage).Add(gHtmlLabel3);
		}
		if (height >= 27 && gHtmlLabel3.Height > num)
		{
			this.Pages(thisPage).Add(new GImage(257, x + width, y));
			this.Pages(thisPage).Add(new GImage(255, x + width, y + height - 32));
			for (int i = y + 30; i + 32 < y + height; i += 30)
			{
				this.Pages(thisPage).Add(new GImage(256, x + width, i));
			}
			GVSlider gVSlider = new GVSlider(254, x + width + 1, y + 1 + 12, 13, height - 2 - 24, 0.0, 0.0, gHtmlLabel3.Height - num, 1.0);
			gVSlider.SetTag("yBase", gHtmlLabel3.Y);
			gVSlider.SetTag("toScroll", gHtmlLabel3);
			gVSlider.SetTag("viewHeight", num);
			gVSlider.OnValueChange = OnScroll;
			this.Pages(thisPage).Add(gVSlider);
			this.Pages(thisPage).Add(new GHotspot(x + width, y, 15, height, gVSlider));
		}
		else
		{
			this.Pages(thisPage).Add(new GImage(257, x + width, y));
			this.Pages(thisPage).Add(new GImage(255, x + width, y + height - 32));
			for (int j = y + 30; j + 32 < y + height; j += 30)
			{
				this.Pages(thisPage).Add(new GImage(256, x + width, j));
			}
			this.Pages(thisPage).Add(new GImage(254, Hues.Grayscale, x + width + 1, y + 1));
		}
	}

	static GServerGump()
	{
		GServerGump.m_LocationCache = new Dictionary<int, LocationCacheEntry>();
		GServerGump.m_FirstChars = new char[2] { '\ufffd', 'o' };
		GServerGump.m_SecondChars = new char[1] { '\'' };
	}
}
