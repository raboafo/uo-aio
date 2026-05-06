namespace UOAIO;

public class ReagentValidator : ItemIDValidator
{
	public static readonly ReagentValidator Validator;

	public ReagentValidator()
		: this(null)
	{
	}

	public ReagentValidator(IItemValidator parent)
		: base(parent, 3962, 3963, 3972, 3973, 3974, 3976, 3980, 3981)
	{
	}

	static ReagentValidator()
	{
		ReagentValidator.Validator = new ReagentValidator();
	}
}
