using UOAIO.Profiles;
using Veritas;

namespace UOAIO;

public class IgnoreList : PersistableObject
{
	public static readonly PersistableType TypeCode;

	private CharacterCollection m_Characters;

	public override PersistableType TypeID => IgnoreList.TypeCode;

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
		return new IgnoreList();
	}

	public IgnoreList()
	{
		this.m_Characters = new CharacterCollection();
	}

	protected override void SerializeChildren(PersistanceWriter op)
	{
		foreach (Character character in this.m_Characters)
		{
			character.Serialize(op);
		}
	}

	protected override void DeserializeChildren(PersistanceReader ip)
	{
		while (ip.HasChild)
		{
			this.m_Characters.Add(ip.GetChild() as Character);
		}
	}

	static IgnoreList()
	{
		IgnoreList.TypeCode = new PersistableType("ignore", Construct, Character.TypeCode);
	}
}
