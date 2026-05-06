using Veritas;

namespace UOAIO.Profiles;

public class SpellIconLayout : GumpLayout
{
	public static readonly PersistableType TypeCode;

	private int m_SpellID;

	public override PersistableType TypeID => SpellIconLayout.TypeCode;

	public int SpellID => this.m_SpellID;

	private static PersistableObject Construct()
	{
		return new SpellIconLayout();
	}

	public override Gump CreateGump()
	{
		return new GSpellIcon(this.m_SpellID);
	}

	public override void Update(Gump g)
	{
		base.Update(g);
		this.m_SpellID = (g as GSpellIcon).m_SpellID;
	}

	protected override void SerializeAttributes(PersistanceWriter op)
	{
		base.SerializeAttributes(op);
		op.SetInt32("id", this.m_SpellID);
	}

	protected override void DeserializeAttributes(PersistanceReader ip)
	{
		base.DeserializeAttributes(ip);
		this.m_SpellID = ip.GetInt32("id");
	}

	static SpellIconLayout()
	{
		SpellIconLayout.TypeCode = new PersistableType("spellIcon", Construct);
	}
}
