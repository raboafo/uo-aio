namespace UOAIO;

public class DragEffect : MovingEffect
{
	protected bool m_Double;

	private VertexCache m_vCache;

	private VertexCache m_vCacheDouble;

	public DragEffect(int itemID, int sourceSerial, int xSource, int ySource, int zSource, int targetSerial, int xTarget, int yTarget, int zTarget, IHue hue, bool shouldDouble)
		: base(sourceSerial, targetSerial, xSource, ySource, zSource, xTarget, yTarget, zTarget, itemID, hue)
	{
		this.m_Double = shouldDouble;
	}

	public override void OnStart()
	{
		base.m_Start = Renderer.m_Frames;
		base.m_Sync = new TimeSync(0.25);
	}

	public override void OnStop()
	{
		base.VCPool.ReleaseInstance(this.m_vCache);
		this.m_vCache = null;
		base.VCPool.ReleaseInstance(this.m_vCache);
		this.m_vCacheDouble = null;
	}

	public override bool Slice()
	{
		double normalized = base.m_Sync.Normalized;
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
		int num2 = (Engine.ScreenHeight >> 1) + (X + Y) * 22 - Z * 4;
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
			num2 -= 30;
		}
		base.GetTarget(out X, out Y, out Z);
		X -= xWorld;
		Y -= yWorld;
		Z -= zWorld;
		int num3 = (Engine.GameWidth >> 1) + (X - Y) * 22;
		int num4 = (Engine.GameHeight >> 1) + (X + Y) * 22 - Z * 4;
		num3 += Engine.GameX;
		num4 += Engine.GameY;
		num3 -= Renderer.m_xScroll;
		num4 -= Renderer.m_yScroll;
		if (base.m_Target != null && base.m_Target.GetType() == typeof(Mobile))
		{
			Mobile mobile2 = (Mobile)base.m_Target;
			if (mobile2.Walking.Count > 0)
			{
				WalkAnimation walkAnimation2 = mobile2.Walking.Peek();
				if (walkAnimation2.Snapshot(ref xOffset, ref yOffset, ref fOffset))
				{
					num3 += xOffset;
					num4 += yOffset;
				}
			}
			num4 -= 30;
		}
		Texture texture = null;
		texture = (base.m_Animated ? ((!Renderer.m_Dead) ? base.m_Hue.GetItem(base.m_ItemID + base.m_Animation[(Renderer.m_Frames - base.m_Start) / base.m_Delay % base.m_FrameCount]) : Hues.Grayscale.GetItem(base.m_ItemID + base.m_Animation[(Renderer.m_Frames - base.m_Start) / base.m_Delay % base.m_FrameCount])) : ((!Renderer.m_Dead) ? base.m_Hue.GetItem(base.m_ItemID) : Hues.Grayscale.GetItem(base.m_ItemID)));
		if (base.m_Source == null)
		{
			num -= texture.Width / 2;
			num2 += 22 - texture.Height;
		}
		else
		{
			num -= texture.xMin + (texture.xMax - texture.xMin) / 2;
			num2 -= texture.yMin + (texture.yMax - texture.yMin) / 2;
		}
		if (base.m_Target == null)
		{
			num3 -= texture.Width / 2;
			num4 += 22 - texture.Height;
		}
		else
		{
			num3 -= texture.xMin + (texture.xMax - texture.xMin) / 2;
			num4 -= texture.yMin + (texture.yMax - texture.yMin) / 2;
		}
		int num5 = num + (int)((double)(num3 - num) * normalized);
		int num6 = num2 + (int)((double)(num4 - num2) * normalized);
		if (this.m_vCache == null)
		{
			this.m_vCache = base.VCPool.GetInstance();
		}
		this.m_vCache.DrawGame(texture, num5, num6);
		if (this.m_Double)
		{
			if (this.m_vCacheDouble == null)
			{
				this.m_vCacheDouble = base.VCPool.GetInstance();
			}
			this.m_vCacheDouble.DrawGame(texture, num5 + 5, num6 + 5);
		}
		return true;
	}
}
