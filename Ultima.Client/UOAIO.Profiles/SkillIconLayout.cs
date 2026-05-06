using Veritas;

namespace UOAIO.Profiles;

public class SkillIconLayout : GumpLayout
{
	public static readonly PersistableType TypeCode;

	private int m_SkillID;

	public override PersistableType TypeID => SkillIconLayout.TypeCode;

	public int SkillID => this.m_SkillID;

	private static PersistableObject Construct()
	{
		return new SkillIconLayout();
	}

	public override void Update(Gump g)
	{
		base.Update(g);
		this.m_SkillID = (g as GSkillIcon).SkillID;
	}

	public override Gump CreateGump()
	{
		return new GSkillIcon(Engine.Skills[this.m_SkillID]);
	}

	protected override void SerializeAttributes(PersistanceWriter op)
	{
		base.SerializeAttributes(op);
		op.SetInt32("id", this.m_SkillID);
	}

	protected override void DeserializeAttributes(PersistanceReader ip)
	{
		base.DeserializeAttributes(ip);
		this.m_SkillID = ip.GetInt32("id");
	}

	static SkillIconLayout()
	{
		SkillIconLayout.TypeCode = new PersistableType("skillIcon", Construct);
	}
}
