using System;

namespace UOAIO;

public class LightningEffect : Effect
{
	private static Texture[] _lightningTextures;

	protected int m_Start;

	protected IHue m_Hue;

	protected VertexCache m_vCache;

	protected Texture m_tCache;

	protected TimeSync _sync;

	public IHue Hue
	{
		get
		{
			return this.m_Hue;
		}
		set
		{
			this.m_Hue = value;
		}
	}

	private static Texture[] GetTextures()
	{
		if (LightningEffect._lightningTextures == null)
		{
			LightningEffect._lightningTextures = Texture.FromImageSet("play/images/lightning.png", 64, 512, 8, 1);
		}
		return LightningEffect._lightningTextures;
	}

	public LightningEffect(IHue Hue)
	{
		this.m_vCache = new VertexCache();
		base.m_Children = new EffectList();
		this.m_Hue = Hue;
	}

	public LightningEffect(int Source, int xSource, int ySource, int zSource, IHue Hue)
		: this(Hue)
	{
		Mobile mobile = World.FindMobile(Source);
		if (mobile != null)
		{
			base.SetSource(mobile);
			return;
		}
		Item item = World.FindItem(Source);
		if (item != null)
		{
			base.SetSource(item);
		}
		else
		{
			base.SetSource(xSource, ySource, zSource);
		}
	}

	public LightningEffect(int xSource, int ySource, int zSource, IHue Hue)
		: this(Hue)
	{
		base.SetSource(xSource, ySource, zSource);
	}

	public LightningEffect(Mobile Source, IHue Hue)
		: this(Hue)
	{
		base.SetSource(Source);
	}

	public LightningEffect(Item Source, IHue Hue)
		: this(Hue)
	{
		base.SetSource(Source);
	}

	public override void OnStart()
	{
		this._sync = new TimeSync(0.5);
		this.m_Start = Renderer.m_Frames;
	}

	public override bool Slice()
	{
		Texture[] textures = LightningEffect.GetTextures();
		double normalized = this._sync.Normalized;
		if (normalized >= 1.0)
		{
			return false;
		}
		int xWorld = Renderer.m_xWorld;
		int yWorld = Renderer.m_yWorld;
		int zWorld = Renderer.m_zWorld;
		int X = 0;
		int Y = 0;
		int Z = 0;
		int xOffset = 0;
		int yOffset = 0;
		int fOffset = 0;
		base.GetSource(out X, out Y, out Z);
		X -= xWorld;
		Y -= yWorld;
		Z -= zWorld;
		int num = (Engine.GameWidth >> 1) + (X - Y) * 22;
		int num2 = (Engine.GameHeight >> 1) + (X + Y) * 22 - Z * 4;
		num += Engine.GameX;
		num2 += Engine.GameY;
		num -= Renderer.m_xScroll;
		num2 -= Renderer.m_yScroll;
		if (base.m_Source != null && base.m_Source.GetType() == typeof(Mobile))
		{
			Mobile mobile = (Mobile)base.m_Source;
			if (mobile.Walking.Count > 0)
			{
				WalkAnimation walkAnimation = mobile.Walking.Peek();
				if (walkAnimation.Snapshot(ref xOffset, ref yOffset, ref fOffset))
				{
					num += xOffset;
					num2 += yOffset;
				}
			}
		}
		int num3 = Math.Max(0, Math.Min(textures.Length, (int)(normalized * (double)textures.Length)));
		Texture texture = textures[num3];
		if (this.m_tCache != texture)
		{
			this.m_tCache = texture;
			this.m_vCache.Invalidate();
		}
		Renderer.SetBlendType(DrawBlendType.Additive);
		Renderer.FilterEnable = true;
		texture.DrawScaled(num, num2 - 5, 16777215, texture.Width / 2, texture.Height - 20, 2f, 2f);
		Renderer.FilterEnable = false;
		Renderer.SetBlendType(DrawBlendType.Normal);
		return true;
	}
}
