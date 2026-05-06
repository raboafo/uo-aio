namespace UOAIO;

public class SkillTooltip : Tooltip
{
	private Skill m_Skill;

	private float m_LastReal;

	private float m_LastUsed;

	public SkillTooltip(Skill s)
		: base("Value: 0.0 (0.0)", Big: true)
	{
		this.m_Skill = s;
		base.m_Delay = 0.5f;
	}

	public override Gump GetGump()
	{
		if (this.m_LastReal != this.m_Skill.Real || this.m_LastUsed != this.m_Skill.Value)
		{
			this.m_LastReal = this.m_Skill.Real;
			this.m_LastUsed = this.m_Skill.Value;
			base.Text = $"Value: {this.m_LastUsed:F1} ({this.m_LastReal:F1})";
		}
		return base.GetGump();
	}
}
