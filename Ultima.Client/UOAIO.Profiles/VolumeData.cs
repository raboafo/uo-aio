using System.Reflection;
using Veritas;

namespace UOAIO.Profiles;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public abstract class VolumeData : PersistableObject
{
	protected Volume m_Volume;

	public Volume Volume => this.m_Volume;

	public VolumeData()
	{
		this.m_Volume = new Volume();
	}

	protected override void SerializeAttributes(PersistanceWriter op)
	{
		op.SetInt32("volume", this.m_Volume.Scale);
		op.SetBoolean("mute", this.m_Volume.Mute);
	}

	protected override void DeserializeAttributes(PersistanceReader ip)
	{
		this.m_Volume.Scale = ip.GetInt32("volume");
		this.m_Volume.Mute = ip.GetBoolean("mute");
	}
}
