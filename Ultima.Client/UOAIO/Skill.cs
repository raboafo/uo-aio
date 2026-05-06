namespace UOAIO;

public class Skill
{
	public string Name;

	public float Value;

	public float Real;

	public int ID;

	public SkillGroup Group;

	public SkillLock Lock;

	public bool Action;

	public Skill(int id, bool action, string name)
	{
		this.ID = id;
		this.Action = action;
		this.Name = name;
		this.Value = 0f;
		this.Real = 0f;
		this.Group = null;
		this.Lock = SkillLock.Up;
	}

	public void Use()
	{
		Network.Send(new PUseSkill(this));
	}
}
