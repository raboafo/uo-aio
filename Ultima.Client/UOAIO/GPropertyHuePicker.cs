using System.Reflection;

namespace UOAIO;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class GPropertyHuePicker : GAlphaBackground
{
	private GHuePicker m_HuePicker;

	private GBrightnessBar m_Bar;

	private GPropertyEntry m_Entry;

	private void HueSelected(int hue, Gump g)
	{
		this.m_Entry.SetValue(hue);
	}

	private void HueReleased(int hue, Gump g)
	{
		this.m_Entry.SetValue(hue);
		Gumps.Destroy(this);
	}

	public GPropertyHuePicker(GPropertyEntry entry)
		: base(0, 0, 200, 150)
	{
		int num = (int)entry.Entry.Property.GetValue(entry.Object, null);
		this.m_Entry = entry;
		base.m_CanDrag = false;
		base.FillColor = GumpColors.Control;
		base.BorderColor = GumpColors.ControlDarkDark;
		base.FillAlpha = 1f;
		GHuePicker gHuePicker = (this.m_HuePicker = new GHuePicker(7, 7));
		gHuePicker.m_CanDrag = false;
		gHuePicker.OnHueSelect = HueSelected;
		base.m_Children.Add(gHuePicker);
		GBrightnessBar gBrightnessBar = (this.m_Bar = new GBrightnessBar(gHuePicker.X + gHuePicker.Width + 1, gHuePicker.Y, 15, gHuePicker.Height, gHuePicker));
		gBrightnessBar.m_CanDrag = false;
		base.m_Children.Add(gBrightnessBar);
		if (num >= 2 && num <= 1001)
		{
			num -= 2;
			gHuePicker.ShadeX = num / 5 % 20;
			gHuePicker.ShadeY = num / 5 / 20;
			gHuePicker.Brightness = num % 5;
			gBrightnessBar.Refresh();
		}
		GSingleBorder toAdd = new GSingleBorder(gBrightnessBar.X - 1, gBrightnessBar.Y, 1, gBrightnessBar.Height);
		base.m_Children.Add(toAdd);
		this.Width = 7 + gHuePicker.Width + gBrightnessBar.Width + 7;
		this.Height = 7 + gHuePicker.Height + 7;
	}

	protected internal override void Draw(int X, int Y)
	{
		base.Draw(X, Y);
		int num = X + this.m_HuePicker.X;
		int num2 = Y + this.m_HuePicker.Y;
		int num3 = this.m_HuePicker.Width + 1 + this.m_Bar.Width;
		int height = this.m_HuePicker.Height;
		Renderer.SetTexture(null);
		GumpPaint.DrawSunken3D(X + this.m_HuePicker.X - 2, Y + this.m_HuePicker.Y - 2, this.m_HuePicker.Width + 1 + this.m_Bar.Width + 4, this.m_HuePicker.Height + 4);
	}
}
