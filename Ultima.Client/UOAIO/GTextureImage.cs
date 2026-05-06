using System.Reflection;

namespace UOAIO;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class GTextureImage : Gump, ITranslucent, IClipable
{
	protected bool m_Draw;

	protected bool m_Invalidated;

	protected Texture m_TextureImage;

	protected int m_Width;

	protected int m_Height;

	protected float m_fAlpha = 1f;

	private VertexCache m_vCache;

	protected Clipper m_Clipper;

	private static VertexCachePool m_vPool;

	protected VertexCachePool VCPool => GTextureImage.m_vPool;

	public Clipper Clipper
	{
		get
		{
			return this.m_Clipper;
		}
		set
		{
			this.m_Clipper = value;
		}
	}

	public Texture TextureImage
	{
		get
		{
			return this.m_TextureImage;
		}
		set
		{
			if (this.m_TextureImage != value)
			{
				this.m_TextureImage = value;
				this.Invalidate();
			}
		}
	}

	public float Alpha
	{
		get
		{
			return this.m_fAlpha;
		}
		set
		{
			this.m_fAlpha = value;
		}
	}

	public Texture Image
	{
		get
		{
			if (this.m_Invalidated)
			{
				this.Refresh();
			}
			return this.m_TextureImage;
		}
	}

	public override int Width
	{
		get
		{
			if (this.m_Invalidated)
			{
				this.Refresh();
			}
			return this.m_Width;
		}
	}

	public override int Height
	{
		get
		{
			if (this.m_Invalidated)
			{
				this.Refresh();
			}
			return this.m_Height;
		}
	}

	protected void Invalidate()
	{
		this.m_Invalidated = true;
	}

	protected void Refresh()
	{
		if (this.m_Invalidated)
		{
			if (this.m_vCache != null)
			{
				this.m_vCache.Invalidate();
			}
			if (this.m_TextureImage != null && !this.m_TextureImage.IsEmpty())
			{
				this.m_Width = this.m_TextureImage.Width;
				this.m_Height = this.m_TextureImage.Height;
				this.m_Draw = true;
			}
			else
			{
				this.m_Width = (this.m_Height = 0);
				this.m_Draw = false;
			}
			this.m_Invalidated = false;
		}
	}

	protected internal override void Draw(int x, int y)
	{
		if (this.m_Invalidated)
		{
			this.Refresh();
		}
		if (!this.m_Draw)
		{
			return;
		}
		Renderer.PushAlpha(this.m_fAlpha);
		if (this.m_Clipper == null)
		{
			if (this.m_vCache == null)
			{
				this.m_vCache = this.VCPool.GetInstance();
			}
			this.m_vCache.Draw(this.m_TextureImage, x, y);
		}
		else
		{
			this.m_TextureImage.DrawClipped(x, y, this.m_Clipper);
		}
		Renderer.PopAlpha();
	}

	protected internal override void OnDispose()
	{
		this.VCPool.ReleaseInstance(this.m_vCache);
		this.m_vCache = null;
	}

	public GTextureImage(Texture textureimage, int x, int y)
		: base(x, y)
	{
		this.m_TextureImage = textureimage;
		this.Invalidate();
		base.m_ITranslucent = true;
	}

	static GTextureImage()
	{
		GTextureImage.m_vPool = new VertexCachePool();
	}
}
