using Veritas;

namespace UOAIO.Profiles;

public class FootstepData : VolumeData
{
	public static readonly PersistableType TypeCode;

	public override PersistableType TypeID => FootstepData.TypeCode;

	[Optionable("Footstep", "Audio")]
	public Volume XYZ
	{
		get
		{
			return base.m_Volume;
		}
		set
		{
		}
	}

	private static PersistableObject Construct()
	{
		return new FootstepData();
	}

	static FootstepData()
	{
		FootstepData.TypeCode = new PersistableType("footstep", Construct);
	}
}
