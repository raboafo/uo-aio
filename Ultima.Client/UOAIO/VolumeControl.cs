using Microsoft.Win32;

namespace UOAIO;

public class VolumeControl
{
	private static int m_Music;

	private static int m_Sound;

	public static int Sound
	{
		get
		{
			if (VolumeControl.m_Sound == int.MinValue)
			{
				VolumeControl.m_Sound = 100;
				try
				{
					using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\KUOC");
					if (registryKey != null)
					{
						VolumeControl.m_Sound = (int)registryKey.GetValue("Sound Volume", 100);
					}
				}
				catch
				{
				}
			}
			return VolumeControl.m_Sound;
		}
		set
		{
			if (VolumeControl.m_Sound == value)
			{
				return;
			}
			VolumeControl.m_Sound = value;
			try
			{
				RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\KUOC", writable: true);
				if (registryKey == null)
				{
					registryKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\KUOC");
				}
				registryKey.SetValue("Sound Volume", value);
				registryKey.Close();
			}
			catch
			{
			}
		}
	}

	public static int Music
	{
		get
		{
			if (VolumeControl.m_Music == int.MinValue)
			{
				VolumeControl.m_Music = 100;
				try
				{
					using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\KUOC");
					if (registryKey != null)
					{
						VolumeControl.m_Music = (int)registryKey.GetValue("Music Volume", 100);
					}
				}
				catch
				{
				}
			}
			return VolumeControl.m_Music;
		}
		set
		{
			if (VolumeControl.m_Music == value)
			{
				return;
			}
			VolumeControl.m_Music = value;
			try
			{
				RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\KUOC", writable: true);
				if (registryKey == null)
				{
					registryKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\KUOC");
				}
				registryKey.SetValue("Music Volume", value);
				registryKey.Close();
			}
			catch
			{
			}
		}
	}

	static VolumeControl()
	{
		VolumeControl.m_Music = int.MinValue;
		VolumeControl.m_Sound = int.MinValue;
	}
}
