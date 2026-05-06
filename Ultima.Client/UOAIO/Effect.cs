namespace UOAIO;

public class Effect
{
	protected object m_Source;

	protected object m_Target;

	protected int m_xSource;

	protected int m_ySource;

	protected int m_zSource;

	protected int m_xTarget;

	protected int m_yTarget;

	protected int m_zTarget;

	protected EffectList m_Children;

	private static VertexCachePool m_vPool;

	protected VertexCachePool VCPool => Effect.m_vPool;

	public EffectList Children => this.m_Children;

	public void SetSource(int X, int Y, int Z)
	{
		this.m_Source = null;
		this.m_xSource = X;
		this.m_ySource = Y;
		this.m_zSource = Z;
	}

	public void SetSource(Mobile Source)
	{
		this.m_Source = Source;
	}

	public void SetSource(Item Source)
	{
		this.m_Source = Source;
	}

	public void SetTarget(int X, int Y, int Z)
	{
		this.m_Target = null;
		this.m_xTarget = X;
		this.m_yTarget = Y;
		this.m_zTarget = Z;
	}

	public void SetTarget(Mobile Target)
	{
		this.m_Target = Target;
	}

	public void SetTarget(Item Target)
	{
		this.m_Target = Target;
	}

	protected void GetSource(out int X, out int Y, out int Z)
	{
		if (this.m_Source == null)
		{
			X = this.m_xSource;
			Y = this.m_ySource;
			Z = this.m_zSource;
		}
		else if (this.m_Source.GetType() == typeof(Mobile))
		{
			Mobile mobile = (Mobile)this.m_Source;
			X = mobile.X;
			Y = mobile.Y;
			Z = mobile.Z;
		}
		else if (this.m_Source.GetType() == typeof(Item))
		{
			Item item = (Item)this.m_Source;
			X = item.X;
			Y = item.Y;
			Z = item.Z;
		}
		else
		{
			X = 0;
			Y = 0;
			Z = 0;
		}
	}

	protected void GetTarget(out int X, out int Y, out int Z)
	{
		if (this.m_Target == null)
		{
			X = this.m_xTarget;
			Y = this.m_yTarget;
			Z = this.m_zTarget;
		}
		else if (this.m_Target.GetType() == typeof(Mobile))
		{
			Mobile mobile = (Mobile)this.m_Target;
			X = mobile.X;
			Y = mobile.Y;
			Z = mobile.Z;
		}
		else if (this.m_Target.GetType() == typeof(Item))
		{
			Item item = (Item)this.m_Target;
			X = item.X;
			Y = item.Y;
			Z = item.Z;
		}
		else
		{
			X = 0;
			Y = 0;
			Z = 0;
		}
	}

	public virtual void OnStart()
	{
	}

	public virtual void OnStop()
	{
	}

	public virtual bool Slice()
	{
		return false;
	}

	public virtual void RenderLight()
	{
	}

	static Effect()
	{
		Effect.m_vPool = new VertexCachePool();
	}
}
