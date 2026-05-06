using Veritas;

namespace UOAIO.Profiles;

public class MusicData : VolumeData
{
	public static readonly PersistableType TypeCode;

	public override PersistableType TypeID => MusicData.TypeCode;

	[Optionable("Music", "Audio")]
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
		return new MusicData();
	}

	static MusicData()
	{
		MusicData.TypeCode = new PersistableType("music", Construct);
	}
}
