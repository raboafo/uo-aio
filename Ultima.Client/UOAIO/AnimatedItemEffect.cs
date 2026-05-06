namespace UOAIO;

public class AnimatedItemEffect : Effect
{
	protected int m_Start;

	protected IHue m_Hue;

	protected int m_ItemID;

	protected bool m_Animated;

	protected sbyte[] m_Animation;

	protected int m_FrameCount;

	protected int m_Delay;

	protected int m_Duration;

	public int m_RenderMode;

	protected VertexCache m_vCache;

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

	public AnimatedItemEffect(int ItemID, IHue Hue, int duration)
	{
		if (ItemID == 14013 && duration == 10)
		{
			duration = 14;
		}
		base.m_Children = new EffectList();
		this.m_Duration = duration;
		this.m_Hue = Hue;
		this.m_ItemID = ItemID;
		this.m_Animated = true;
		AnimData anim = Map.GetAnim(ItemID);
		this.m_FrameCount = anim.frameCount;
		this.m_Delay = anim.frameInterval;
		this.m_Animation = new sbyte[64];
		for (int i = 0; i < 64; i++)
		{
			this.m_Animation[i] = anim[i];
		}
		if (this.m_FrameCount == 0)
		{
			this.m_FrameCount = 1;
			this.m_Animation[0] = 0;
		}
		if (this.m_Delay == 0)
		{
			this.m_Delay = 1;
		}
	}

	public override void RenderLight()
	{
		if (Renderer.m_Frames - this.m_Start >= this.m_Duration)
		{
			return;
		}
		this.GetRenderLocation(out var xPixel, out var yPixel);
		xPixel -= Engine.GameX;
		yPixel -= Engine.GameY;
		int num = this.m_ItemID;
		if (this.m_Animated)
		{
			num += this.m_Animation[(Renderer.m_Frames - this.m_Start) / this.m_Delay % this.m_FrameCount];
		}
		if (this.m_ItemID != 14000 && this.m_ItemID != 14013)
		{
			return;
		}
		int num2 = num - this.m_ItemID;
		if (num2 >= 0 && num2 < 12)
		{
			yPixel -= 65;
			float num3 = (float)(Renderer.m_Frames - this.m_Start) / (float)this.m_Duration;
			float num4 = num3 * 2f;
			if (num4 > 1f)
			{
				num4 = 2f - num4;
			}
			Renderer.RenderLight(this.m_Start, xPixel, yPixel, num & 0x3FFF, (num2 < 3) ? 1 : 29, 0, num4);
		}
	}

	public AnimatedItemEffect(int Source, int ItemID, IHue Hue, int duration)
		: this(Source, 0, 0, 0, ItemID, Hue, duration)
	{
	}

	public AnimatedItemEffect(int Source, int xSource, int ySource, int zSource, int ItemID, IHue Hue, int duration)
		: this(ItemID, Hue, duration)
	{
		Mobile mobile = World.FindMobile(Source);
		if (mobile != null)
		{
			base.SetSource(mobile);
			if (!mobile.Player && !mobile.IsMoving && (xSource != 0 || ySource != 0 || zSource != 0))
			{
				mobile.SetLocation(xSource, ySource, zSource);
				mobile.Update();
				mobile.UpdateReal();
			}
			return;
		}
		Item item = World.FindItem(Source);
		if (item != null)
		{
			base.SetSource(item);
			if (xSource != 0 || ySource != 0 || zSource != 0)
			{
				item.SetLocation(xSource, ySource, zSource);
				item.Update();
			}
		}
		else
		{
			base.SetSource(xSource, ySource, zSource);
		}
	}

	public AnimatedItemEffect(int xSource, int ySource, int zSource, int ItemID, IHue Hue, int duration)
		: this(ItemID, Hue, duration)
	{
		base.SetSource(xSource, ySource, zSource);
	}

	public AnimatedItemEffect(Mobile Source, int ItemID, IHue Hue, int duration)
		: this(ItemID, Hue, duration)
	{
		base.SetSource(Source);
	}

	public AnimatedItemEffect(Item Source, int ItemID, IHue Hue, int duration)
		: this(ItemID, Hue, duration)
	{
		base.SetSource(Source);
	}

	public override void OnStart()
	{
		this.m_Start = Renderer.m_Frames;
	}

	public override void OnStop()
	{
		base.VCPool.ReleaseInstance(this.m_vCache);
		this.m_vCache = null;
	}

	private void GetRenderLocation(out int xPixel, out int yPixel)
	{
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
		xPixel = (Engine.GameWidth >> 1) + (X - Y) * 22;
		yPixel = (Engine.GameHeight >> 1) + 22 + (X + Y) * 22 - Z * 4;
		xPixel += Engine.GameX;
		yPixel += Engine.GameY;
		if (base.m_Source != null && base.m_Source.GetType() == typeof(Mobile))
		{
			Mobile mobile = (Mobile)base.m_Source;
			if (mobile.Walking.Count > 0)
			{
				WalkAnimation walkAnimation = mobile.Walking.Peek();
				if (walkAnimation.Snapshot(ref xOffset, ref yOffset, ref fOffset))
				{
					xPixel += xOffset;
					yPixel += yOffset;
				}
			}
		}
		xPixel -= Renderer.m_xScroll;
		yPixel -= Renderer.m_yScroll;
	}

	public override bool Slice()
	{
		if (Renderer.m_Frames - this.m_Start >= this.m_Duration)
		{
			return false;
		}
		this.GetRenderLocation(out var xPixel, out var yPixel);
		Texture texture = null;
		IHue hue = (Renderer.m_Dead ? Hues.Grayscale : this.m_Hue);
		int num = this.m_ItemID;
		if (this.m_Animated)
		{
			num += this.m_Animation[(Renderer.m_Frames - this.m_Start) / this.m_Delay % this.m_FrameCount];
		}
		texture = hue.GetItem(num);
		if (this.m_vCache == null)
		{
			this.m_vCache = base.VCPool.GetInstance();
		}
		float num2 = (Renderer.m_Frames - this.m_Start) / this.m_Duration;
		RenderEffect renderEffect = Effects.GetItemEffect(num);
		if (renderEffect != null && renderEffect.Glow != null)
		{
			Texture item = Hues.Shadow.GetItem(num);
			if (item != null && !item.IsEmpty())
			{
				Renderer.PushAlpha(renderEffect.Glow.Alpha);
				Renderer.SetBlendType(DrawBlendType.Additive);
				int color = hue.Pixel32(renderEffect.Glow.Color ?? item._averageColor);
				item.DrawGame(xPixel - texture.Width / 2 - 8, yPixel - texture.Height - 8, color);
				if (renderEffect.Glow.Color == 9203350)
				{
					Renderer.SetAlpha(renderEffect.Glow.Alpha * 0.5f);
					item.DrawGame(xPixel - texture.Width / 2 - 8, yPixel - texture.Height - 8, color);
				}
				Renderer.SetBlendType(DrawBlendType.Normal);
				Renderer.PopAlpha();
			}
		}
		switch (this.m_RenderMode)
		{
		case 2:
		case 3:
			renderEffect = RenderEffects.Additive;
			break;
		case 4:
			renderEffect = RenderEffects.HalfAdditive;
			break;
		}
		if (renderEffect == null)
		{
			renderEffect = RenderEffects.Default;
		}
		Renderer.PushAlpha(renderEffect.Alpha);
		Renderer.SetBlendType(renderEffect.BlendType);
		this.m_vCache.DrawGame(texture, xPixel - texture.Width / 2, yPixel - texture.Height);
		Renderer.SetBlendType(DrawBlendType.Normal);
		Renderer.PopAlpha();
		return true;
	}
}
