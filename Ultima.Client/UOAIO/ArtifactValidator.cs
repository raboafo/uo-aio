namespace UOAIO;

public class ArtifactValidator : IItemValidator
{
	public static readonly ArtifactValidator Default;

	private IItemValidator m_Parent;

	public ArtifactValidator()
		: this(null)
	{
	}

	public ArtifactValidator(IItemValidator parent)
	{
		this.m_Parent = parent;
	}

	public bool IsValid(Item check)
	{
		if (this.m_Parent != null && !this.m_Parent.IsValid(check))
		{
			return false;
		}
		ObjectPropertyList propertyList = check.PropertyList;
		if (propertyList == null)
		{
			return false;
		}
		for (int i = 0; i < propertyList.Properties.Length; i++)
		{
			if (propertyList.Properties[i].Number == 1061078)
			{
				return true;
			}
		}
		return false;
	}

	static ArtifactValidator()
	{
		ArtifactValidator.Default = new ArtifactValidator();
	}
}
