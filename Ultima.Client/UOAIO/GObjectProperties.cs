using System;
using System.Collections;

namespace UOAIO;

public class GObjectProperties : GAlphaBackground
{
	private static GObjectProperties m_Instance;

	private object m_Object;

	private ObjectPropertyList m_List;

	public bool m_WorldTooltip;

	private int m_TotalHeight;

	private int m_TotalWidth;

	private Timer m_Timer;

	private TimeSync m_Sync;

	private double m_WidthDuration;

	private double m_HeightDuration;

	private int m_CompactHeight;

	public static GObjectProperties Instance => GObjectProperties.m_Instance;

	public object Object => this.m_Object;

	public override int Width
	{
		get
		{
			return base.m_Width;
		}
		set
		{
			base.m_Width = value;
			Gump[] array = base.m_Children.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				GLabel gLabel = (GLabel)array[i];
				gLabel.Scissor(1 - gLabel.X, 1 - gLabel.Y, this.Width - 2, this.Height - 2);
			}
		}
	}

	public override int Height
	{
		get
		{
			return base.m_Height;
		}
		set
		{
			base.m_Height = value;
			Gump[] array = base.m_Children.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				GLabel gLabel = (GLabel)array[i];
				gLabel.Scissor(1 - gLabel.X, 1 - gLabel.Y, this.Width - 2, this.Height - 2);
			}
		}
	}

	protected void Roll_OnTick(Timer t)
	{
		double elapsed = this.m_Sync.Elapsed;
		if (elapsed < this.m_WidthDuration)
		{
			double num = elapsed / this.m_WidthDuration;
			this.Width = 2 + (int)(num * (double)(this.m_TotalWidth - 2));
			this.Height = this.m_CompactHeight;
		}
		else
		{
			double num = (elapsed - this.m_WidthDuration) / this.m_HeightDuration;
			if (num >= 1.0)
			{
				if (this.m_Timer != null)
				{
					this.m_Timer.Stop();
				}
				this.m_Timer = null;
				this.Width = this.m_TotalWidth;
				this.Height = this.m_TotalHeight;
			}
			else
			{
				this.Width = this.m_TotalWidth;
				this.Height = this.m_CompactHeight + (int)(num * (double)(this.m_TotalHeight - this.m_CompactHeight));
			}
		}
		Engine.Redraw();
	}

	protected internal override bool HitTest(int X, int Y)
	{
		return false;
	}

	public GObjectProperties(int number, object o, ObjectPropertyList propList)
		: base(0, 0, 2, 20)
	{
		this.m_Object = o;
		base.m_CanDrag = false;
		this.m_Timer = new Timer(Roll_OnTick, 0);
		this.m_Timer.Start(Now: false);
		int num = propList.Properties.Length;
		if (num == 0 && number != -1)
		{
			num = 1;
		}
		this.SetList(number, propList);
		this.m_WidthDuration = (double)this.m_TotalWidth * 0.000625;
		this.m_HeightDuration = (double)num * 0.0125;
		this.m_Sync = new TimeSync(this.m_WidthDuration + this.m_HeightDuration);
	}

	private IHue GetDefaultHue()
	{
		if (this.m_Object is Mobile)
		{
			return Hues.GetNotoriety(((Mobile)this.m_Object).Notoriety);
		}
		return Hues.Load(52);
	}

	public void SetList(int number, ObjectPropertyList propList)
	{
		this.m_List = propList;
		base.m_Children.Clear();
		int num = 5;
		int num2 = 10;
		ObjectProperty[] array = propList.Properties;
		if (array.Length == 0 && number != -1)
		{
			array = new ObjectProperty[1]
			{
				new ObjectProperty(number, "")
			};
		}
		int num3 = 0;
		int num4 = -1;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		int num9 = 0;
		int num10 = 0;
		int num11 = 0;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Number >= 1060445 && array[i].Number <= 1060449)
			{
				int num12 = 0;
				try
				{
					num12 = int.Parse(array[i].Arguments.Trim());
				}
				catch
				{
				}
				switch (array[i].Number)
				{
				case 1060445:
					num7 += num12;
					break;
				case 1060446:
					num9 += num12;
					break;
				case 1060447:
					num6 += num12;
					break;
				case 1060448:
					num5 += num12;
					break;
				case 1060449:
					num8 += num12;
					break;
				}
				if (num4 == -1)
				{
					num4 = i;
				}
				num3 += num12;
			}
			else if (array[i].Number == 1060486)
			{
				try
				{
					num11 = int.Parse(array[i].Arguments.Trim());
				}
				catch
				{
				}
			}
			else if (array[i].Number == 1061167)
			{
				try
				{
					num10 = int.Parse(array[i].Arguments.Trim());
				}
				catch
				{
				}
			}
		}
		if (num10 > 0)
		{
			num10 *= 100 + num11;
			num10 /= 100;
		}
		ResistInfo resistInfo = null;
		ResistInfo resistInfo2 = null;
		if (num4 != -1)
		{
			ArrayList arrayList = new ArrayList(array);
			arrayList.Insert(num4, new ObjectProperty(1042971, $"total resist {num3}%"));
			array = (ObjectProperty[])arrayList.ToArray(typeof(ObjectProperty));
			resistInfo = ResistInfo.Find(array[0].Text, ResistInfo.m_Armor);
			resistInfo2 = ResistInfo.Find(array[0].Text, ResistInfo.m_Materials);
		}
		int num13 = -1;
		if (this.m_Object is Mobile)
		{
			Mobile mobile = (Mobile)this.m_Object;
			int num14 = 0;
			int num15 = 0;
			int num16 = 0;
			int num17 = 0;
			int num18 = 0;
			int num19 = 0;
			int num20 = 0;
			foreach (Item item2 in mobile.Items)
			{
				ObjectPropertyList propertyList = item2.PropertyList;
				if (propertyList == null)
				{
					item2.QueryProperties();
					continue;
				}
				ObjectProperty[] properties = propertyList.Properties;
				foreach (ObjectProperty objectProperty in properties)
				{
					if (objectProperty.Number >= 1060445 && objectProperty.Number <= 1060449)
					{
						int num21 = 0;
						try
						{
							num21 = int.Parse(objectProperty.Arguments.Trim());
						}
						catch
						{
						}
						switch (objectProperty.Number)
						{
						case 1060445:
							num16 += num21;
							break;
						case 1060446:
							num18 += num21;
							break;
						case 1060447:
							num15 += num21;
							break;
						case 1060448:
							num14 += num21;
							break;
						case 1060449:
							num17 += num21;
							break;
						}
					}
					else if (objectProperty.Number == 1060413)
					{
						try
						{
							num19 += int.Parse(objectProperty.Arguments.Trim());
						}
						catch
						{
						}
					}
					else if (objectProperty.Number == 1060412)
					{
						try
						{
							num20 += int.Parse(objectProperty.Arguments.Trim());
						}
						catch
						{
						}
					}
				}
			}
			num13 = num14;
			if (num15 < num13)
			{
				num13 = num15;
			}
			if (num16 < num13)
			{
				num13 = num16;
			}
			if (num17 < num13)
			{
				num13 = num17;
			}
			if (num18 < num13)
			{
				num13 = num18;
			}
			if (num14 != 0 || num15 != 0 || num16 != 0 || num17 != 0 || num18 != 0 || num19 != 0 || num20 != 0)
			{
				ArrayList arrayList2 = new ArrayList(array);
				if (num19 != 0 || num20 != 0)
				{
					arrayList2.Add(new ObjectProperty(1042971, $"FC {num19}, FCR {num20}"));
				}
				arrayList2.Add(new ObjectProperty(1060448, num14.ToString()));
				arrayList2.Add(new ObjectProperty(1060447, num15.ToString()));
				arrayList2.Add(new ObjectProperty(1060445, num16.ToString()));
				arrayList2.Add(new ObjectProperty(1060449, num17.ToString()));
				arrayList2.Add(new ObjectProperty(1060446, num18.ToString()));
				array = (ObjectProperty[])arrayList2.ToArray(typeof(ObjectProperty));
			}
		}
		if (this.m_Object is Item && ((Item)this.m_Object).Container != null && ((Item)this.m_Object).Container.m_TradeContainer)
		{
			ArrayList arrayList3 = new ArrayList(array);
			if (arrayList3.Count > 0)
			{
				arrayList3[0] = new ObjectProperty(1042971, "Their Offer");
			}
			Item item = (Item)this.m_Object;
			Item[] array2 = item.FindItems(new ItemIDValidator(3821, 3820, 3822));
			int num22 = 0;
			for (int k = 0; k < array2.Length; k++)
			{
				num22 += array2[k].Amount;
			}
			Item[] array3 = item.FindItems(new ItemIDValidator(5359, 5360));
			for (int l = 0; l < array3.Length; l++)
			{
				ObjectPropertyList propertyList2 = array3[l].PropertyList;
				if (propertyList2 == null)
				{
					array3[l].QueryProperties();
					continue;
				}
				bool flag = false;
				for (int m = 0; m < propertyList2.Properties.Length; m++)
				{
					if (propertyList2.Properties[m].Number == 1041361)
					{
						flag = true;
					}
					else if (propertyList2.Properties[m].Number == 1060738 && flag)
					{
						try
						{
							num22 += int.Parse(propertyList2.Properties[m].Arguments.Trim());
						}
						catch
						{
						}
						break;
					}
				}
			}
			arrayList3.Add(new ObjectProperty(1042971, $"Total Gold: {num22:N0}"));
			array = (ObjectProperty[])arrayList3.ToArray(typeof(ObjectProperty));
		}
		for (int n = 0; n < array.Length; n++)
		{
			ObjectProperty objectProperty2 = array[n];
			GLabel gLabel = new GLabel(Engine.MakeProperCase(objectProperty2.Text), Engine.DefaultFont, (n == 0) ? this.GetDefaultHue() : Hues.Bright, 5, num);
			if (objectProperty2.Number == 1061170)
			{
				int num23 = 0;
				try
				{
					num23 = int.Parse(objectProperty2.Arguments.Trim());
				}
				catch
				{
				}
				Mobile player = World.Player;
				if (player != null && num23 > player.Strength)
				{
					gLabel.Hue = Hues.Load(34);
				}
			}
			else if (objectProperty2.Number == 1061682)
			{
				gLabel.Hue = Hues.Load(89);
				gLabel.Text = "Insured";
			}
			else if (num13 >= 0 && objectProperty2.Number >= 1060445 && objectProperty2.Number <= 1060449)
			{
				int num24 = 0;
				try
				{
					num24 = int.Parse(objectProperty2.Arguments.Trim());
				}
				catch
				{
				}
				if (num24 == num13)
				{
					if (objectProperty2.Number == 1060445)
					{
						gLabel.Hue = Hues.Load(95);
					}
					else if (objectProperty2.Number == 1060446)
					{
						gLabel.Hue = Hues.Load(26);
					}
					else if (objectProperty2.Number == 1060447)
					{
						gLabel.Hue = Hues.Load(45);
					}
					else if (objectProperty2.Number == 1060448)
					{
						gLabel.Hue = Hues.Load(1154);
					}
					else if (objectProperty2.Number == 1060449)
					{
						gLabel.Hue = Hues.Load(70);
					}
				}
			}
			else if (resistInfo != null && objectProperty2.Number >= 1060445 && objectProperty2.Number <= 1060449)
			{
				int num25 = 0;
				try
				{
					num25 = int.Parse(objectProperty2.Arguments.Trim());
				}
				catch
				{
				}
				int num26 = 0;
				switch (objectProperty2.Number)
				{
				case 1060445:
					num26 = resistInfo.m_Cold + (resistInfo2?.m_Cold ?? 0);
					break;
				case 1060446:
					num26 = resistInfo.m_Energy + (resistInfo2?.m_Energy ?? 0);
					break;
				case 1060447:
					num26 = resistInfo.m_Fire + (resistInfo2?.m_Fire ?? 0);
					break;
				case 1060448:
					num26 = resistInfo.m_Physical + (resistInfo2?.m_Physical ?? 0);
					break;
				case 1060449:
					num26 = resistInfo.m_Poison + (resistInfo2?.m_Poison ?? 0);
					break;
				}
				int num27 = num25 - num26;
				if (num27 > 0)
				{
					gLabel.Text += $" (+{num27}%)";
				}
				else if (num27 < 0)
				{
					gLabel.Text += $" (-{Math.Abs(num27)}%)";
				}
			}
			else if (objectProperty2.Number == 1061167)
			{
				Mobile player2 = World.Player;
				if (player2 != null)
				{
					double num28 = Math.Floor(40000.0 / (double)((player2.CurrentStamina + 100) * num10)) / 2.0;
					gLabel.Text += $" ({num28:F1}s)";
				}
			}
			gLabel.Y -= gLabel.Image.yMin;
			gLabel.X -= gLabel.Image.xMin;
			num = gLabel.Y + gLabel.Image.yMax + 5;
			if (10 + (gLabel.Image.xMax - gLabel.Image.xMin + 1) > num2)
			{
				num2 = 10 + (gLabel.Image.xMax - gLabel.Image.xMin + 1);
			}
			base.m_Children.Add(gLabel);
			if (n == 0)
			{
				this.m_CompactHeight = num;
			}
		}
		this.m_TotalWidth = num2;
		this.m_TotalHeight = num;
		if (this.m_Timer == null)
		{
			this.Width = num2;
			this.Height = num;
		}
		Gump[] array4 = base.m_Children.ToArray();
		for (int num29 = 0; num29 < array4.Length; num29++)
		{
			GLabel gLabel2 = (GLabel)array4[num29];
			gLabel2.X = (num2 - (gLabel2.Image.xMax - gLabel2.Image.xMin + 1)) / 2 - gLabel2.Image.xMin;
			gLabel2.Scissor(1 - gLabel2.X, 1 - gLabel2.Y, this.Width - 2, this.Height - 2);
		}
	}

	protected internal override void Render(int X, int Y)
	{
		if (this.m_Object is Item)
		{
			Item item = (Item)this.m_Object;
			if (item.PropertyList == null)
			{
				item.QueryProperties();
			}
			else if (item.PropertyList != this.m_List)
			{
				this.SetList(1020000 + item.ID, item.PropertyList);
			}
		}
		if (this.m_WorldTooltip)
		{
			bool flag = Engine.m_xMouse < Engine.ScreenWidth / 2;
			bool flag2 = Engine.m_yMouse < Engine.ScreenHeight / 2;
			int num = Engine.m_xMouse - this.Width - 2;
			int num2 = Engine.m_yMouse - this.Height - 2;
			if (flag)
			{
				num = ((!flag2) ? Engine.m_xMouse : (Engine.m_xMouse + Cursor.Width + 2));
			}
			if (flag2)
			{
				num2 = ((!flag) ? Engine.m_yMouse : (Engine.m_yMouse + Cursor.Height + 2));
			}
			if (num < 2)
			{
				num = 2;
			}
			else if (num + this.Width + 2 > Engine.ScreenWidth)
			{
				num = Engine.ScreenWidth - this.Width - 2;
			}
			if (num2 < 2)
			{
				num2 = 2;
			}
			else if (num2 + this.Height + 2 > Engine.ScreenHeight)
			{
				num2 = Engine.ScreenHeight - this.Height - 2;
			}
			this.X = num;
			this.Y = num2;
		}
		base.Render(X, Y);
	}

	public static void Hide()
	{
		if (GObjectProperties.m_Instance != null)
		{
			Gumps.Destroy(GObjectProperties.m_Instance);
		}
		GObjectProperties.m_Instance = null;
	}

	public static void Display(object o)
	{
		GObjectProperties.Hide();
		if (o is Item)
		{
			Item item = (Item)o;
			ObjectPropertyList propertyList = item.PropertyList;
			if (propertyList != null)
			{
				GObjectProperties.m_Instance = new GObjectProperties(1020000 + item.ID, o, propertyList);
			}
		}
		else if (o is Mobile)
		{
			Mobile mobile = (Mobile)o;
			ObjectPropertyList propertyList2 = mobile.PropertyList;
			if (propertyList2 != null)
			{
				GObjectProperties.m_Instance = new GObjectProperties(-1, mobile, propertyList2);
			}
		}
		if (GObjectProperties.m_Instance != null)
		{
			Gumps.Desktop.Children.Add(GObjectProperties.m_Instance);
			GObjectProperties.m_Instance.m_WorldTooltip = true;
		}
	}
}
