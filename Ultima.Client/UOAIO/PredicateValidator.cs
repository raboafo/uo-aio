using System;

namespace UOAIO;

public class PredicateValidator : IItemValidator
{
	private IItemValidator m_Parent;

	private Predicate<Item> predicate;

	public PredicateValidator(Predicate<Item> predicate)
		: this(null, predicate)
	{
	}

	public PredicateValidator(IItemValidator parent, Predicate<Item> predicate)
	{
		this.m_Parent = parent;
		this.predicate = predicate;
	}

	public bool IsValid(Item check)
	{
		if (this.m_Parent != null && !this.m_Parent.IsValid(check))
		{
			return false;
		}
		return this.predicate(check);
	}
}
