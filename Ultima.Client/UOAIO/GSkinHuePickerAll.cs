using System.Windows.Forms;

namespace UOAIO;

public class GSkinHuePickerAll : Gump
{
	private const int xShades = 7;

	private const int yShades = 1;

	private const int zShades = 8;

	private int xSize;

	private int ySize;

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

	public int Hue => 1002 + (this.m_yShade * 7 + this.m_xShade) * 8 + this.m_zShade;

	public override int Width => this.xSize * 7;

	public override int Height => this.ySize * 8;

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

	private void ChangeShade(int X, int Y)
	{
		int xShade = this.m_xShade;
		int yShade = this.m_yShade;
		int zShade = this.m_zShade;
		this.m_xShade = X / this.xSize;
		this.m_yShade = Y / this.ySize;
		this.m_zShade = this.m_yShade / 1;
		this.m_yShade %= 1;
		if (this.m_xShade != xShade || this.m_yShade != yShade || this.m_zShade != zShade)
		{
			if (this.m_OnHueSelect != null)
			{
				this.m_OnHueSelect(this.Hue, this);
			}
			Engine.Redraw();
		}
	}

	public GSkinHuePickerAll(int X, int Y, int Width, int Height)
		: base(X, Y)
	{
		do
		{
			this.xSize++;
		}
		while (this.xSize * 7 <= Width);
		this.xSize--;
		do
		{
			this.ySize++;
		}
		while (this.ySize * 8 <= Height);
		this.ySize--;
		this.m_ColorTable = new int[7, 1, 8];
		for (int i = 0; i < 7; i++)
		{
			for (int j = 0; j < 1; j++)
			{
				for (int k = 0; k < 8; k++)
				{
					ushort num = Hues.GetData(1001 + (j * 7 + i) * 8 + k).colors[48];
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
		for (int i = 0; i < 7; i++)
		{
			for (int j = 0; j < 1; j++)
			{
				for (int k = 0; k < 8; k++)
				{
					Renderer.SolidRect(this.m_ColorTable[i, j, k], X + i * this.xSize, Y + (k + j) * this.ySize, this.xSize, this.ySize);
				}
			}
		}
		Renderer.SolidRect(8454143, X + this.m_xShade * this.xSize + (this.xSize - 3) / 2, Y + (this.m_zShade + this.m_yShade) * this.ySize + (this.ySize - 1) / 2, 3, 1);
		Renderer.SolidRect(8454143, X + this.m_xShade * this.xSize + (this.xSize - 1) / 2, Y + (this.m_zShade + this.m_yShade) * this.ySize + (this.ySize - 3) / 2, 1, 3);
	}
}
