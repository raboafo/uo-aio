using Veritas;

namespace UOAIO.Profiles;

public abstract class GumpLayout : PersistableObject
{
	protected Point m_Offset;

	public Point Offset
	{
		get
		{
			return this.m_Offset;
		}
		set
		{
			this.m_Offset = value;
		}
	}

	public GumpLayout()
	{
	}

	public virtual void Remove()
	{
		Preferences.Current.Layout.Gumps.Remove(this);
	}

	public virtual void BringToTop()
	{
		Preferences.Current.Layout.Gumps.Remove(this);
		Preferences.Current.Layout.Gumps.Add(this);
	}

	public virtual void Update(Gump g)
	{
		this.m_Offset = new Point(g.X, g.Y);
	}

	public virtual void Setup(Gump g)
	{
		g.X = this.m_Offset.X;
		g.Y = this.m_Offset.Y;
	}

	public abstract Gump CreateGump();

	protected override void SerializeAttributes(PersistanceWriter op)
	{
		op.SetPoint("gump", this.m_Offset);
	}

	protected override void DeserializeAttributes(PersistanceReader ip)
	{
		this.m_Offset = ip.GetPoint("gump");
	}
}
