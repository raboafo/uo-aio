using Veritas;

namespace UOAIO.Profiles;

public class Friends : PersistableObject
{
	public static readonly PersistableType TypeCode;

	private CharacterCollection m_Characters;

	public override PersistableType TypeID => Friends.TypeCode;

	public CharacterCollection Characters => this.m_Characters;

	public Character this[Mobile mob]
	{
		get
		{
			if (mob != null)
			{
				foreach (Character character in this.m_Characters)
				{
					if (character.Serial == mob.Serial)
					{
						return character;
					}
				}
			}
			return null;
		}
	}

	private static PersistableObject Construct()
	{
		return new Friends();
	}

	public Friends()
	{
		this.m_Characters = new CharacterCollection();
	}

	protected override void SerializeChildren(PersistanceWriter op)
	{
		for (int i = 0; i < this.m_Characters.Count; i++)
		{
			this.m_Characters[i].Serialize(op);
		}
	}

	protected override void DeserializeChildren(PersistanceReader ip)
	{
		while (ip.HasChild)
		{
			this.m_Characters.Add(ip.GetChild() as Character);
		}
	}

	static Friends()
	{
		Friends.TypeCode = new PersistableType("friends", Construct, Character.TypeCode);
	}
}
