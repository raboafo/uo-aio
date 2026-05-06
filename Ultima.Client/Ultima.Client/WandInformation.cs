namespace Ultima.Client;

internal struct WandInformation
{
	private readonly WandEffect effect;

	private readonly int charges;

	public WandEffect Effect => this.effect;

	public int Charges => this.charges;

	public WandInformation(WandEffect effect, int charges)
	{
		this.effect = effect;
		this.charges = charges;
	}

	public static WandEffect? GetEffectByLabel(int number)
	{
		return number switch
		{
			1044063 => WandEffect.Identification, 
			3002011 => WandEffect.Clumsiness, 
			3002013 => WandEffect.Feeblemindedness, 
			3002014 => WandEffect.Healing, 
			3002015 => WandEffect.MagicArrow, 
			3002018 => WandEffect.Weakness, 
			3002022 => WandEffect.Harming, 
			3002028 => WandEffect.Fireball, 
			3002039 => WandEffect.GreaterHealing, 
			3002040 => WandEffect.Lightning, 
			3002041 => WandEffect.ManaDraining, 
			_ => null, 
		};
	}
}
