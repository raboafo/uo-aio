namespace UOAIO;

public sealed class Faction
{
	private string friendlyName;

	private string abbreviation;

	public static readonly Faction Minax;

	public static readonly Faction CouncilOfMages;

	public static readonly Faction Shadowlords;

	public static readonly Faction TrueBritannians;

	public string FriendlyName => this.friendlyName;

	public string Abbreviation => this.abbreviation;

	public Faction(string friendlyName, string abbreviation)
	{
		this.friendlyName = friendlyName;
		this.abbreviation = abbreviation;
	}

	static Faction()
	{
		Faction.Minax = new Faction("Minax", "Minax");
		Faction.CouncilOfMages = new Faction("Council of Mages", "CoM");
		Faction.Shadowlords = new Faction("Shadowlords", "SL");
		Faction.TrueBritannians = new Faction("True Britannians", "TB");
	}
}
