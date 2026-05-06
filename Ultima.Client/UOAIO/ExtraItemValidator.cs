namespace UOAIO;

public class ExtraItemValidator : ItemIDValidator
{
	public static readonly ExtraItemValidator Vailidator;

	public ExtraItemValidator()
		: this(null)
	{
	}

	public ExtraItemValidator(IItemValidator parent)
		: base(parent, 3617)
	{
	}

	static ExtraItemValidator()
	{
		ExtraItemValidator.Vailidator = new ExtraItemValidator();
	}
}
