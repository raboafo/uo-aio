using Veritas;

namespace UOAIO.Profiles;

public class Character : PersistableObject
{
	public static readonly PersistableType TypeCode;

	private int m_Serial;

	private string m_Name;

	public override PersistableType TypeID => Character.TypeCode;

	public int Serial => this.m_Serial;

	public string Name
	{
		get
		{
			return this.m_Name;
		}
		set
		{
			this.m_Name = value;
		}
	}

	public bool IsNull => this.m_Name == null && this.m_Serial == 0;

	private static PersistableObject Construct()
	{
		return new Character();
	}

	public Mobile Find()
	{
		return World.FindMobile(this.m_Serial);
	}

	protected Character()
	{
	}

	public Character(Mobile mob)
	{
		if (mob != null)
		{
			this.m_Name = mob.Name;
			this.m_Serial = mob.Serial;
		}
	}

	protected override void SerializeAttributes(PersistanceWriter op)
	{
		if (!this.IsNull)
		{
			op.SetString("name", this.m_Name);
			op.SetInt32("serial", this.m_Serial);
		}
	}

	protected override void DeserializeAttributes(PersistanceReader ip)
	{
		this.m_Name = ip.GetString("name");
		this.m_Serial = ip.GetInt32("serial");
	}

	static Character()
	{
		Character.TypeCode = new PersistableType("character", Construct);
	}
}
