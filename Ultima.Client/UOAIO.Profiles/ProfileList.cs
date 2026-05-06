using Veritas;

namespace UOAIO.Profiles;

public class ProfileList : PersistableObject
{
	public static readonly PersistableType TypeCode;

	private ProfileCollection m_Profiles;

	public override PersistableType TypeID => ProfileList.TypeCode;

	public Profile this[string name]
	{
		get
		{
			Profile profile;
			for (int i = 0; i < this.m_Profiles.Count; i++)
			{
				profile = this.m_Profiles[i];
				if (profile.Name == name)
				{
					return profile;
				}
			}
			this.m_Profiles.Add(profile = new Profile(name));
			return profile;
		}
	}

	private static PersistableObject Construct()
	{
		return new ProfileList();
	}

	public ProfileList()
	{
		this.m_Profiles = new ProfileCollection();
	}

	protected override void SerializeChildren(PersistanceWriter op)
	{
		for (int i = 0; i < this.m_Profiles.Count; i++)
		{
			this.m_Profiles[i].Serialize(op);
		}
	}

	protected override void DeserializeChildren(PersistanceReader ip)
	{
		while (ip.HasChild)
		{
			this.m_Profiles.Add(ip.GetChild() as Profile);
		}
	}

	static ProfileList()
	{
		ProfileList.TypeCode = new PersistableType("profiles", Construct, Profile.TypeCode);
	}
}
