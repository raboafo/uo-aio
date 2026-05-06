using System.Drawing;
using Veritas;

namespace UOAIO.Profiles;

public abstract class SizableLayout : GumpLayout
{
	protected Size m_Size;

	public Size Size
	{
		get
		{
			return this.m_Size;
		}
		set
		{
			this.m_Size = value;
		}
	}

	public override void Setup(Gump g)
	{
		base.Setup(g);
		g.Width = this.m_Size.Width;
		g.Height = this.m_Size.Height;
	}

	protected override void SerializeAttributes(PersistanceWriter op)
	{
		base.SerializeAttributes(op);
		op.SetSize("gump", this.m_Size);
	}

	protected override void DeserializeAttributes(PersistanceReader ip)
	{
		base.DeserializeAttributes(ip);
		this.m_Size = ip.GetSize("gump");
	}
}
