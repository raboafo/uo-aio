using System;
using System.Collections;

namespace UOAIO;

public class GHtmlLabel : Gump
{
	private int m_Width;

	private int m_Height;

	private static object[,] m_ColorTable;

	public override int Width
	{
		get
		{
			return this.m_Width;
		}
		set
		{
			this.m_Width = value;
		}
	}

	public override int Height
	{
		get
		{
			return this.m_Height;
		}
		set
		{
			this.m_Height = value;
		}
	}

	public void Update(string text, UnicodeFont initialFont, int initialColor)
	{
		int width = this.m_Width;
		Stack stack = new Stack();
		FontInfo fontInfo = new FontInfo(initialFont, initialColor);
		int num = 0;
		int num2 = 0;
		Stack stack2 = new Stack();
		Stack stack3 = new Stack();
		text = text.Replace("\r", "");
		text = text.Replace("\n", "<br>");
		HtmlElement[] elements = HtmlElement.GetElements(text);
		int num3 = 0;
		int num4 = 0;
		foreach (HtmlElement htmlElement in elements)
		{
			FontInfo fontInfo2 = ((stack.Count > 0) ? ((FontInfo)stack.Peek()) : fontInfo);
			HtmlAlignment htmlAlignment = ((stack2.Count > 0) ? ((HtmlAlignment)stack2.Peek()) : HtmlAlignment.Normal);
			string text2 = ((stack3.Count > 0) ? ((string)stack3.Peek()) : null);
			switch (htmlElement.Type)
			{
			case ElementType.Text:
			{
				string text3 = htmlElement.Name;
				bool flag = false;
				while (text3.Length > 0)
				{
					int num7 = num3;
					switch (htmlAlignment & (HtmlAlignment)255)
					{
					case HtmlAlignment.Center:
						num7 = fontInfo2.Font.GetStringWidth(text3);
						if (num7 > width)
						{
							string text5 = Engine.WrapText(text3, width, fontInfo2.Font);
							string[] array2 = text5.Split('\n');
							num7 = fontInfo2.Font.GetStringWidth(array2[0]);
						}
						num7 = (width - (num7 - 1) + 1) / 2;
						break;
					case HtmlAlignment.Right:
						num7 = fontInfo2.Font.GetStringWidth(text3);
						if (num7 > width)
						{
							string text4 = Engine.WrapText(text3, width, fontInfo2.Font);
							string[] array = text4.Split('\n');
							num7 = fontInfo2.Font.GetStringWidth(array[0]);
						}
						num7 = ((int)htmlAlignment >> 8) - num7;
						break;
					case HtmlAlignment.Left:
						num7 = (int)htmlAlignment >> 8;
						break;
					}
					string[] array3 = text3.Split(' ');
					int num8 = width - num7;
					if (!flag && fontInfo2.Font.GetStringWidth(array3[0]) > num8)
					{
						flag = true;
						num3 = 0;
						num4 += 18;
						continue;
					}
					flag = false;
					string text6 = Engine.WrapText(text3, num8, fontInfo2.Font);
					array3 = text6.Split('\n');
					string text7 = array3[0];
					if (array3.Length > 1)
					{
						text7 = text7.TrimEnd();
					}
					GLabel gLabel = ((text2 == null) ? new GLabel(text7, fontInfo2.Font, fontInfo2.Hue, num7, num4) : new GHyperLink(text2, text7, fontInfo2.Font, num7, num4));
					if (text2 == null)
					{
						gLabel.Underline = num > 0;
					}
					base.m_Children.Add(gLabel);
					if (array3.Length > 1)
					{
						text3 = text3.Remove(0, array3[0].Length);
						num3 = 0;
						num4 += 18;
						continue;
					}
					num3 = gLabel.X + gLabel.Width - 1;
					break;
				}
				break;
			}
			case ElementType.Start:
				switch (htmlElement.Name.ToLower())
				{
				case "br":
					num3 = 0;
					num4 += 18;
					break;
				case "u":
					num++;
					break;
				case "i":
					num2++;
					break;
				case "a":
				{
					string attribute3 = htmlElement.GetAttribute("href");
					if (attribute3 != null && !attribute3.StartsWith("?"))
					{
						stack3.Push(attribute3);
					}
					break;
				}
				case "basefont":
				{
					string attribute2 = htmlElement.GetAttribute("color");
					if (attribute2 == null)
					{
						break;
					}
					int color = 0;
					if (attribute2.StartsWith("#"))
					{
						color = Convert.ToInt32(attribute2.Substring(1), 16);
					}
					else
					{
						for (int j = 0; j < GHtmlLabel.m_ColorTable.GetLength(0); j++)
						{
							if (attribute2.ToLower() == (string)GHtmlLabel.m_ColorTable[j, 0])
							{
								color = (int)GHtmlLabel.m_ColorTable[j, 1];
								break;
							}
						}
					}
					stack.Push(new FontInfo(fontInfo2.Font, color));
					break;
				}
				case "center":
					stack2.Push(HtmlAlignment.Center);
					break;
				case "div":
				{
					string attribute = htmlElement.GetAttribute("align");
					if (attribute == null)
					{
						attribute = htmlElement.GetAttribute("alignleft");
						if (attribute != null)
						{
							try
							{
								int num5 = int.Parse(attribute);
								stack2.Push((HtmlAlignment)(2 | (num5 << 8)));
							}
							catch
							{
							}
						}
						attribute = htmlElement.GetAttribute("alignright");
						if (attribute != null)
						{
							try
							{
								int num6 = int.Parse(attribute);
								stack2.Push((HtmlAlignment)(3 | (num6 << 8)));
							}
							catch
							{
							}
						}
					}
					else
					{
						switch (attribute.ToLower())
						{
						case "center":
							stack2.Push(HtmlAlignment.Center);
							break;
						case "right":
							stack2.Push((HtmlAlignment)(3 | (width << 8)));
							break;
						case "left":
							stack2.Push(HtmlAlignment.Left);
							break;
						}
					}
					break;
				}
				}
				break;
			case ElementType.End:
				switch (htmlElement.Name.ToLower())
				{
				case "u":
					num--;
					if (num < 0)
					{
						num = 0;
					}
					break;
				case "i":
					num2--;
					if (num2 < 0)
					{
						num2 = 0;
					}
					break;
				case "a":
					if (stack3.Count > 0)
					{
						stack3.Pop();
					}
					break;
				case "basefont":
					if (stack.Count > 0)
					{
						stack.Pop();
					}
					break;
				case "div":
				case "center":
					if (stack2.Count > 0)
					{
						stack2.Pop();
					}
					break;
				}
				break;
			}
		}
		this.m_Height = num4 + 18;
	}

	public GHtmlLabel(string text, UnicodeFont initialFont, int initialColor, int x, int y, int width)
		: base(x, y)
	{
		this.m_Width = width;
		this.Update(text, initialFont, initialColor);
	}

	public void Scissor(int x, int y, int w, int h)
	{
		Gump[] array = base.m_Children.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			GLabel gLabel = (GLabel)array[i];
			gLabel.Scissor(x - gLabel.X, y - gLabel.Y, w, h);
		}
	}

	static GHtmlLabel()
	{
		GHtmlLabel.m_ColorTable = new object[24, 2]
		{
			{ "black", 0 },
			{ "red", 16711680 },
			{ "lime", 65280 },
			{ "blue", 255 },
			{ "yellow", 16776960 },
			{ "magenta", 16711935 },
			{ "fuchsia", 16711935 },
			{ "cyan", 65535 },
			{ "aqua", 65535 },
			{ "white", 16777215 },
			{ "darkred", 8323072 },
			{ "maroon", 8323072 },
			{ "green", 32512 },
			{ "darkgreen", 23040 },
			{ "darkblue", 32512 },
			{ "navy", 32512 },
			{ "darkyellow", 8355584 },
			{ "olive", 8355584 },
			{ "darkmagenta", 8323199 },
			{ "purple", 8323199 },
			{ "darkcyan", 32639 },
			{ "teal", 32639 },
			{ "gray", 8355711 },
			{ "grey", 8355711 }
		};
	}
}
