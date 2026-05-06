using Veritas;

namespace UOAIO.Profiles;

public class SoundData : VolumeData
{
	public static readonly PersistableType TypeCode;

	public override PersistableType TypeID => SoundData.TypeCode;

	[Optionable("Sound", "Audio")]
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
		return new SoundData();
	}

	static SoundData()
	{
		SoundData.TypeCode = new PersistableType("sound", Construct);
	}
}
