namespace UOAIO;

public class GBorder3D : GAlphaBackground
{
	protected bool m_Inset;

	public bool Inset
	{
		get
		{
			return this.m_Inset;
		}
		set
		{
			this.m_Inset = value;
		}
	}

	public GBorder3D(bool inset, int x, int y, int width, int height)
		: base(x, y, width, height)
	{
		this.m_Inset = inset;
		base.FillAlpha = 1f;
		base.FillColor = 12632256;
	}

	protected internal override void Draw(int X, int Y)
	{
		Renderer.SetTexture(null);
		Renderer.PushAlpha(base.m_FillAlpha);
		if (base.m_FillAlpha == 1f)
		{
			Renderer.SolidRect(base.m_FillColor, X + 1, Y + 1, base.m_Width - 2, base.m_Height - 2);
		}
		else
		{
			Renderer.SolidRect(base.m_FillColor, X + 1, Y + 1, base.m_Width - 2, base.m_Height - 2);
		}
		Renderer.PopAlpha();
		Renderer.SolidRect(this.m_Inset ? 4210752 : 16777215, X, Y, base.m_Width - 1, 1);
		Renderer.SolidRect(this.m_Inset ? 4210752 : 16777215, X, Y + 1, 1, base.m_Height - 2);
		Renderer.SolidRect(12632256, X, Y + base.m_Height - 1, 1, 1);
		Renderer.SolidRect(12632256, X + base.m_Width - 1, Y, 1, 1);
		Renderer.SolidRect(this.m_Inset ? 16777215 : 4210752, X + 1, Y + base.m_Height - 1, base.m_Width - 1, 1);
		Renderer.SolidRect(this.m_Inset ? 16777215 : 4210752, X + base.m_Width - 1, Y + 1, 1, base.m_Height - 2);
	}
}
