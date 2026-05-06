using System.Windows.Forms;

namespace UOAIO;

public class GHuePicker : Gump
{
	private const int xShades = 20;

	private const int yShades = 10;

	private const int zShades = 5;

	private const int xSize = 8;

	private const int ySize = 8;

	private int m_xShade;

	private int m_yShade;

	private int m_zShade;

	private int[,,] m_ColorTable;

	private OnHueSelect m_OnHueRelease;

	private OnHueSelect m_OnHueSelect;

	public OnHueSelect OnHueSelect
	{
		get
		{
			return this.m_OnHueSelect;
		}
		set
		{
			this.m_OnHueSelect = value;
		}
	}

	public OnHueSelect OnHueRelease
	{
		get
		{
			return this.m_OnHueRelease;
		}
		set
		{
			this.m_OnHueRelease = value;
		}
	}

	public int ShadeX
	{
		set
		{
			this.m_xShade = value;
			if (this.m_xShade < 0)
			{
				this.m_xShade = 0;
			}
			if (this.m_xShade >= 20)
			{
				this.m_xShade = 19;
			}
			if (this.m_OnHueSelect != null)
			{
				this.m_OnHueSelect(this.Hue, this);
			}
			if (Engine.GMPrivs)
			{
				((Tooltip)base.m_Tooltip).Text = $"0x{this.Hue:X}";
			}
			Engine.Redraw();
		}
	}

	public int ShadeY
	{
		set
		{
			this.m_yShade = value;
			if (this.m_yShade < 0)
			{
				this.m_yShade = 0;
			}
			if (this.m_yShade >= 10)
			{
				this.m_yShade = 9;
			}
			if (this.m_OnHueSelect != null)
			{
				this.m_OnHueSelect(this.Hue, this);
			}
			if (Engine.GMPrivs)
			{
				((Tooltip)base.m_Tooltip).Text = $"0x{this.Hue:X}";
			}
			Engine.Redraw();
		}
	}

	public int Brightness
	{
		get
		{
			return this.m_zShade;
		}
		set
		{
			this.m_zShade = value;
			if (this.m_zShade < 0)
			{
				this.m_zShade = 0;
			}
			if (this.m_zShade >= 5)
			{
				this.m_zShade = 4;
			}
			if (this.m_OnHueSelect != null)
			{
				this.m_OnHueSelect(this.Hue, this);
			}
			if (Engine.GMPrivs)
			{
				((Tooltip)base.m_Tooltip).Text = $"0x{this.Hue:X}";
			}
			Engine.Redraw();
		}
	}

	public int Hue => 2 + (this.m_yShade * 20 + this.m_xShade) * 5 + this.m_zShade;

	public override int Width => 160;

	public override int Height => 80;

	public int Color(int Index)
	{
		return this.m_ColorTable[this.m_xShade, this.m_yShade, Index];
	}

	protected internal override bool HitTest(int X, int Y)
	{
		return true;
	}

	protected internal override void OnMouseDown(int X, int Y, MouseButtons mb)
	{
		this.ChangeShade(X, Y);
	}

	protected internal override void OnMouseUp(int X, int Y, MouseButtons mb)
	{
		this.ChangeShade(X, Y);
		if (this.m_OnHueRelease != null)
		{
			this.m_OnHueRelease(this.Hue, this);
		}
	}

	protected internal override void OnMouseMove(int X, int Y, MouseButtons mb)
	{
		if (mb != MouseButtons.None)
		{
			this.ChangeShade(X, Y);
		}
	}

	private int ColorAt(int X, int Y)
	{
		if (X >= 20)
		{
			X = 19;
		}
		if (Y >= 10)
		{
			Y = 9;
		}
		return this.m_ColorTable[X, Y, this.m_zShade];
	}

	private void ChangeShade(int X, int Y)
	{
		int xShade = this.m_xShade;
		int yShade = this.m_yShade;
		this.m_xShade = X / 8;
		this.m_yShade = Y / 8;
		if (this.m_xShade != xShade || this.m_yShade != yShade)
		{
			if (this.m_OnHueSelect != null)
			{
				this.m_OnHueSelect(this.Hue, this);
			}
			if (Engine.GMPrivs)
			{
				((Tooltip)base.m_Tooltip).Text = $"0x{this.Hue:X}";
			}
			Engine.Redraw();
		}
	}

	public GHuePicker(int X, int Y)
		: base(X, Y)
	{
		base.m_Tooltip = new Tooltip("");
		this.m_ColorTable = new int[20, 10, 5];
		for (int i = 0; i < 20; i++)
		{
			for (int j = 0; j < 10; j++)
			{
				for (int k = 0; k < 5; k++)
				{
					ushort num = Hues.GetData(1 + (j * 20 + i) * 5 + k).colors[48];
					int num2 = (num >> 10) & 0x1F;
					int num3 = (num >> 5) & 0x1F;
					int num4 = num & 0x1F;
					num2 = (int)((float)num2 * 8.225806f);
					num3 = (int)((float)num3 * 8.225806f);
					num4 = (int)((float)num4 * 8.225806f);
					int num5 = (num2 << 16) | (num3 << 8) | num4;
					this.m_ColorTable[i, j, k] = num5;
				}
			}
		}
	}

	protected internal override void Draw(int X, int Y)
	{
		Renderer.SetTexture(null);
		int num = 4;
		int num2 = 8 - num;
		int num3 = 4;
		int num4 = 8 - num3;
		int color = this.ColorAt(0, 0);
		Renderer.SolidRect(color, X, Y, num, num3);
		color = this.ColorAt(0, 10);
		Renderer.SolidRect(color, X, Y + 80 - num4, num, num4);
		color = this.ColorAt(20, 0);
		Renderer.SolidRect(color, X + 160 - num2, Y, num2, num3);
		color = this.ColorAt(20, 10);
		Renderer.SolidRect(color, X + 160 - num2, Y + 80 - num4, num2, num4);
		for (int i = 0; i < 19; i++)
		{
			int num5 = this.ColorAt(i, 0);
			int num6 = this.ColorAt(i + 1, 0);
			Renderer.GradientRect4(num5, num6, num6, num5, X + i * 8 + num, Y, 8, num3);
			num5 = this.ColorAt(i, 10);
			num6 = this.ColorAt(i + 1, 10);
			Renderer.GradientRect4(num5, num6, num6, num5, X + i * 8 + num, Y + 80 - num4, 8, num4);
			for (int j = 0; j < 9; j++)
			{
				int c = this.ColorAt(i, j);
				int c2 = this.ColorAt(i + 1, j);
				int c3 = this.ColorAt(i + 1, j + 1);
				int c4 = this.ColorAt(i, j + 1);
				int num7 = X + i * 8;
				int num8 = Y + j * 8;
				Renderer.GradientRect4(c, c2, c3, c4, num7 + num, num8 + num3, 8, 8);
			}
		}
		for (int k = 0; k < 9; k++)
		{
			int num9 = this.ColorAt(0, k);
			int num10 = this.ColorAt(0, k + 1);
			Renderer.GradientRect4(num9, num9, num10, num10, X, Y + k * 8 + num3, num, 8);
			num9 = this.ColorAt(20, k);
			num10 = this.ColorAt(20, k + 1);
			Renderer.GradientRect4(num9, num9, num10, num10, X + 160 - num2, Y + k * 8 + num3, num2, 8);
		}
		Renderer.PushAlpha(0.5f);
		Renderer.SolidRect(8454143, X + this.m_xShade * 8 + 2, Y + this.m_yShade * 8 + 3, 3, 1);
		Renderer.SolidRect(8454143, X + this.m_xShade * 8 + 3, Y + this.m_yShade * 8 + 2, 1, 3);
		Renderer.PopAlpha();
		Renderer.SolidRect(8454143, X + this.m_xShade * 8 + 3, Y + this.m_yShade * 8 + 3, 1, 1);
	}
}
