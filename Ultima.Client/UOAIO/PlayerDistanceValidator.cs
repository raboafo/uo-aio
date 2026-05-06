namespace UOAIO;

public class PlayerDistanceValidator : IItemValidator
{
	private int m_Range;

	private IItemValidator m_Parent;

	public PlayerDistanceValidator(int range)
		: this(null, range)
	{
	}

	public PlayerDistanceValidator(IItemValidator parent, int range)
	{
		this.m_Parent = parent;
		this.m_Range = range;
	}

	public bool IsValid(Item check)
	{
		return (this.m_Parent == null || this.m_Parent.IsValid(check)) && check.InRange(World.Player, this.m_Range);
	}
}
