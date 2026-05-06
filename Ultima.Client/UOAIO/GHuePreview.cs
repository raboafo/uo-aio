namespace UOAIO;

public class GHuePreview : Gump
{
	private int m_Width;

	private int m_Height;

	private int m_Hue;

	private bool m_Solid;

	private float m_xRun;

	private int[] m_Colors;

	public int Hue
	{
		get
		{
			return this.m_Hue;
		}
		set
		{
			if (this.m_Hue == value)
			{
				return;
			}
			this.m_Hue = value;
			if (!this.m_Solid)
			{
				this.m_xRun = 31f / (float)this.Width;
				this.m_Colors = new int[32];
				for (int i = 0; i < 32; i++)
				{
					ushort num = Hues.GetData((this.m_Hue - 1) & 0x7FFF).colors[32 + i];
					int num2 = (num >> 10) & 0x1F;
					int num3 = (num >> 5) & 0x1F;
					int num4 = num & 0x1F;
					num2 = (int)((float)num2 * 8.225806f);
					num3 = (int)((float)num3 * 8.225806f);
					num4 = (int)((float)num4 * 8.225806f);
					int num5 = (num2 << 16) | (num3 << 8) | num4;
					this.m_Colors[i] = num5;
				}
			}
			else
			{
				ushort num6 = Hues.GetData((this.m_Hue - 1) & 0x7FFF).colors[48];
				int num7 = (num6 >> 10) & 0x1F;
				int num8 = (num6 >> 5) & 0x1F;
				int num9 = num6 & 0x1F;
				num7 = (int)((float)num7 * 8.225806f);
				num8 = (int)((float)num8 * 8.225806f);
				num9 = (int)((float)num9 * 8.225806f);
				int num10 = (num7 << 16) | (num8 << 8) | num9;
				this.m_Colors = new int[1] { num10 };
			}
		}
	}

	public override int Width => this.m_Width;

	public override int Height => this.m_Height;

	public GHuePreview(int X, int Y, int Width, int Height, int Hue, bool Solid)
		: base(X, Y)
	{
		this.m_Width = Width;
		this.m_Height = Height;
		this.m_Hue = Hue;
		this.m_Solid = Solid;
		if (!Solid)
		{
			this.m_xRun = 31f / (float)Width;
			this.m_Colors = new int[32];
			for (int i = 0; i < 32; i++)
			{
				ushort num = Hues.GetData((Hue - 1) & 0x7FFF).colors[32 + i];
				int num2 = (num >> 10) & 0x1F;
				int num3 = (num >> 5) & 0x1F;
				int num4 = num & 0x1F;
				num2 = (int)((float)num2 * 8.225806f);
				num3 = (int)((float)num3 * 8.225806f);
				num4 = (int)((float)num4 * 8.225806f);
				int num5 = (num2 << 16) | (num3 << 8) | num4;
				this.m_Colors[i] = num5;
			}
		}
		else
		{
			ushort num6 = Hues.GetData((Hue - 1) & 0x7FFF).colors[48];
			int num7 = (num6 >> 10) & 0x1F;
			int num8 = (num6 >> 5) & 0x1F;
			int num9 = num6 & 0x1F;
			num7 = (int)((float)num7 * 8.225806f);
			num8 = (int)((float)num8 * 8.225806f);
			num9 = (int)((float)num9 * 8.225806f);
			int num10 = (num7 << 16) | (num8 << 8) | num9;
			this.m_Colors = new int[1] { num10 };
		}
	}

	protected internal override void Draw(int X, int Y)
	{
		Renderer.SetTexture(null);
		if (this.m_Solid)
		{
			Renderer.SolidRect(this.m_Colors[0], X, Y, this.m_Width, this.m_Height);
		}
		else
		{
			Renderer.GradientRectLR(this.m_Colors[0], this.m_Colors[31], X, Y, this.m_Width, this.m_Height);
		}
	}
}
