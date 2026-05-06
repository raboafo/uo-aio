using System.Collections.Generic;

namespace UOAIO;

public class SkillGroup
{
	public string Name;

	public int GroupID;

	public List<Skill> Skills;

	public SkillGroup(string name, int groupID)
	{
		this.Name = name;
		this.GroupID = groupID;
		this.Skills = new List<Skill>();
	}
}
