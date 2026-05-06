namespace UOAIO;

public class PotionValidator : ItemIDValidator
{
	public static readonly PotionValidator Validator;

	public PotionValidator()
		: this(null)
	{
	}

	public PotionValidator(IItemValidator parent)
		: base(parent, 3847, 3848, 3849, 3851, 3852, 3853)
	{
	}

	static PotionValidator()
	{
		PotionValidator.Validator = new PotionValidator();
	}
}
