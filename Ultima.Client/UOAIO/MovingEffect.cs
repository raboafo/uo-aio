using System;
using Ultima.Data;

namespace UOAIO;

public class MovingEffect : Effect
{
	protected int _effectId;

	protected int m_Start;

	protected IHue m_Hue;

	protected TimeSync m_Sync;

	protected int m_ItemID;

	protected bool m_Animated;

	protected sbyte[] m_Animation;

	protected int m_FrameCount;

	protected int m_Delay;

	public int m_RenderMode;

	public int EffectId
	{
		get
		{
			return this._effectId;
		}
		set
		{
			this._effectId = value;
		}
	}

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

	public MovingEffect(int ItemID, IHue Hue)
	{
		base.m_Children = new EffectList();
		this.m_Hue = Hue;
		this.m_ItemID = ItemID;
		this.m_Animated = Map.m_ItemFlags[ItemID][TileFlag.Animation];
		if (this.m_Animated)
		{
			AnimData anim = Map.GetAnim(ItemID);
			this.m_FrameCount = anim.frameCount;
			this.m_Delay = anim.frameInterval;
			this.m_Animation = new sbyte[64];
			for (int i = 0; i < 64; i++)
			{
				this.m_Animation[i] = anim[i];
			}
			if (this.m_Delay == 0)
			{
				this.m_Delay = 1;
			}
			this.m_Animated = this.m_FrameCount > 0;
		}
	}

	public MovingEffect(int Source, int Target, int xSource, int ySource, int zSource, int xTarget, int yTarget, int zTarget, int ItemID, IHue Hue)
		: this(ItemID, Hue)
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
		}
		else
		{
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
		mobile = World.FindMobile(Target);
		if (mobile != null)
		{
			base.SetTarget(mobile);
			if (!mobile.Player && !mobile.IsMoving && (xTarget != 0 || yTarget != 0 || zTarget != 0))
			{
				mobile.SetLocation(xTarget, yTarget, zTarget);
				mobile.Update();
				mobile.UpdateReal();
			}
			return;
		}
		Item item2 = World.FindItem(Target);
		if (item2 != null)
		{
			base.SetTarget(item2);
			if (xTarget != 0 || yTarget != 0 || zTarget != 0)
			{
				item2.SetLocation(xTarget, yTarget, zTarget);
				item2.Update();
			}
		}
		else
		{
			base.SetTarget(xTarget, yTarget, zTarget);
		}
	}

	public MovingEffect(int xSource, int ySource, int zSource, int xTarget, int yTarget, int zTarget, int ItemID, IHue Hue)
		: this(ItemID, Hue)
	{
		base.SetSource(xSource, ySource, zSource);
		base.SetTarget(xTarget, yTarget, zTarget);
	}

	public MovingEffect(int xSource, int ySource, int zSource, Mobile Target, int ItemID, IHue Hue)
		: this(ItemID, Hue)
	{
		base.SetSource(xSource, ySource, zSource);
		base.SetTarget(Target);
	}

	public MovingEffect(int xSource, int ySource, int zSource, Item Target, int ItemID, IHue Hue)
		: this(ItemID, Hue)
	{
		base.SetSource(xSource, ySource, zSource);
		base.SetTarget(Target);
	}

	public MovingEffect(Mobile Source, int xTarget, int yTarget, int zTarget, int ItemID, IHue Hue)
		: this(ItemID, Hue)
	{
		base.SetSource(Source);
		base.SetTarget(xTarget, yTarget, zTarget);
	}

	public MovingEffect(Mobile Source, Mobile Target, int ItemID, IHue Hue)
		: this(ItemID, Hue)
	{
		base.SetSource(Source);
		base.SetTarget(Target);
	}

	public MovingEffect(Mobile Source, Item Target, int ItemID, IHue Hue)
		: this(ItemID, Hue)
	{
		base.SetSource(Source);
		base.SetTarget(Target);
	}

	public MovingEffect(Item Source, int xTarget, int yTarget, int zTarget, int ItemID, IHue Hue)
		: this(ItemID, Hue)
	{
		base.SetSource(Source);
		base.SetTarget(xTarget, yTarget, zTarget);
	}

	public MovingEffect(Item Source, Mobile Target, int ItemID, IHue Hue)
		: this(ItemID, Hue)
	{
		base.SetSource(Source);
		base.SetTarget(Target);
	}

	public MovingEffect(Item Source, Item Target, int ItemID, IHue Hue)
		: this(ItemID, Hue)
	{
		base.SetSource(Source);
		base.SetTarget(Target);
	}

	public override void OnStart()
	{
		double duration = ((this.m_ItemID == 3853) ? 1.0 : ((base.m_Source != null || this._effectId != 9501) ? 0.5 : (0.1 + Engine.Random.NextDouble() * 0.4)));
		this.m_Start = Renderer.m_Frames;
		this.m_Sync = new TimeSync(duration);
	}

	public override void OnStop()
	{
		base.OnStop();
		if (this._effectId == 9501)
		{
			Engine.Sounds.PlaySound(285);
		}
	}

	public override void RenderLight()
	{
		double normalized = this.m_Sync.Normalized;
		if (!(normalized < 1.0))
		{
			return;
		}
		int itemID = this.m_ItemID;
		this.GetRenderLocation(normalized, out var _, out var _, out var _, out var _, out var xPixel, out var yPixel);
		xPixel -= Engine.GameX;
		yPixel -= Engine.GameY;
		switch (itemID)
		{
		default:
			if (itemID < 14052 || itemID > 14067)
			{
				break;
			}
			goto case 14036;
		case 14036:
		case 14037:
		case 14038:
		case 14039:
		case 14040:
		case 14041:
		case 14042:
		case 14043:
		case 14044:
		case 14045:
		case 14046:
		case 14047:
		case 14048:
		case 14049:
		case 14050:
		case 14051:
		case 14239:
			Renderer.RenderLight(this.m_Start, xPixel, yPixel, itemID, 1);
			break;
		}
	}

	private void GetRenderLocation(double n, out int xSource, out int ySource, out int xTarget, out int yTarget, out int xPixel, out int yPixel)
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
		xSource = (Engine.GameWidth >> 1) + (X - Y) * 22;
		ySource = (Engine.GameHeight >> 1) + (X + Y) * 22 - Z * 4;
		if (base.m_Source is Mobile)
		{
			ySource -= 30;
		}
		xSource += Engine.GameX;
		ySource += Engine.GameY;
		xSource -= Renderer.m_xScroll;
		ySource -= Renderer.m_yScroll;
		if (base.m_Source != null && base.m_Source.GetType() == typeof(Mobile))
		{
			Mobile mobile = (Mobile)base.m_Source;
			if (mobile.Walking.Count > 0)
			{
				WalkAnimation walkAnimation = mobile.Walking.Peek();
				if (walkAnimation.Snapshot(ref xOffset, ref yOffset, ref fOffset))
				{
					xSource += xOffset;
					ySource += yOffset;
				}
			}
		}
		base.GetTarget(out X, out Y, out Z);
		X -= xWorld;
		Y -= yWorld;
		Z -= zWorld;
		xTarget = (Engine.GameWidth >> 1) + (X - Y) * 22;
		yTarget = (Engine.GameHeight >> 1) + (X + Y) * 22 - Z * 4;
		if (base.m_Target is Mobile)
		{
			yTarget -= 50;
		}
		xTarget += Engine.GameX;
		yTarget += Engine.GameY;
		xTarget -= Renderer.m_xScroll;
		yTarget -= Renderer.m_yScroll;
		if (base.m_Target != null && base.m_Target.GetType() == typeof(Mobile))
		{
			Mobile mobile2 = (Mobile)base.m_Target;
			if (mobile2.Walking.Count > 0)
			{
				WalkAnimation walkAnimation2 = mobile2.Walking.Peek();
				if (walkAnimation2.Snapshot(ref xOffset, ref yOffset, ref fOffset))
				{
					xTarget += xOffset;
					yTarget += yOffset;
				}
			}
		}
		xPixel = xSource + (int)((double)(xTarget - xSource) * n);
		yPixel = ySource + (int)((double)(yTarget - ySource) * n);
	}

	public override bool Slice()
	{
		double normalized = this.m_Sync.Normalized;
		if (normalized >= 1.0)
		{
			return false;
		}
		this.GetRenderLocation(normalized, out var xSource, out var ySource, out var xTarget, out var yTarget, out var xPixel, out var yPixel);
		Texture texture = null;
		IHue hue = (Renderer.m_Dead ? Hues.Grayscale : this.m_Hue);
		int num = this.m_ItemID;
		if (this.m_Animated)
		{
			num += this.m_Animation[(Renderer.m_Frames - this.m_Start) / this.m_Delay % this.m_FrameCount];
		}
		texture = hue.GetItem(num);
		double num2 = Math.Atan2(ySource - yTarget, xSource - xTarget);
		double num3 = (double)(texture.xMin + texture.xMax) * 0.5;
		double num4 = (double)(texture.yMin + texture.yMax) * 0.5;
		Renderer.FilterEnable = true;
		if (num == 3853)
		{
			num2 += normalized * Math.PI * 4.0;
		}
		else if (this._effectId == 9501)
		{
			num3 = 12.0;
			num4 = 10.0;
		}
		RenderEffect renderEffect = Effects.GetItemEffect(num);
		int input;
		if (renderEffect != null && renderEffect.Glow != null)
		{
			Texture item = Hues.Shadow.GetItem(num);
			if (item != null && !item.IsEmpty())
			{
				Renderer.PushAlpha(renderEffect.Glow.Alpha);
				Renderer.SetBlendType(DrawBlendType.Additive);
				input = renderEffect.Glow.Color ?? item._averageColor;
				if (this._effectId == 9501)
				{
					input = 16737792;
				}
				input = hue.Pixel32(input);
				item.DrawRotated(xPixel, yPixel, num2, input, num3 + 8.0, num4 + 8.0);
				if (renderEffect.Glow.Color == 9203350)
				{
					Renderer.SetAlpha(renderEffect.Glow.Alpha * 0.5f);
					item.DrawRotated(xPixel, yPixel, num2, input, num3 + 8.0, num4 + 8.0);
				}
				Renderer.SetBlendType(DrawBlendType.Normal);
				Renderer.PopAlpha();
			}
		}
		float num5 = 1f;
		if (this._effectId == 9501)
		{
			Texture item2 = hue.GetItem(4972);
			if (item2 != null && !item2.IsEmpty())
			{
				Renderer.FilterEnable = true;
				item2.DrawRotated(xPixel, yPixel, num2 + normalized * Math.PI * 4.0, 13395507);
				Renderer.FilterEnable = false;
				num5 = 0.5f;
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
		Renderer.PushAlpha(renderEffect.Alpha * num5);
		Renderer.SetBlendType(renderEffect.BlendType);
		Renderer.FilterEnable = true;
		input = 16777215;
		if (this._effectId == 9501)
		{
			input = 16737792;
		}
		texture.DrawRotated(xPixel, yPixel, num2, input, num3, num4);
		Renderer.FilterEnable = false;
		Renderer.SetBlendType(DrawBlendType.Normal);
		Renderer.PopAlpha();
		return true;
	}
}
