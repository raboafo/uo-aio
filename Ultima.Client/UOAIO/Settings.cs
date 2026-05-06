using System.Configuration;
using System.Diagnostics;

namespace UOAIO;

internal sealed class Settings : ApplicationSettingsBase
{
	private static Settings defaultInstance;

	public static Settings Default => Settings.defaultInstance;

	[DefaultSettingValue("")]
	[UserScopedSetting]
	[DebuggerNonUserCode]
	public string Account
	{
		get
		{
			return (string)this["Account"];
		}
		set
		{
			this["Account"] = value;
		}
	}

	[DefaultSettingValue("")]
	[UserScopedSetting]
	[DebuggerNonUserCode]
	public string Password
	{
		get
		{
			return (string)this["Password"];
		}
		set
		{
			this["Password"] = value;
		}
	}

	[DefaultSettingValue("RememberPassword")]
	[UserScopedSetting]
	[DebuggerNonUserCode]
	public bool RememberPassword
	{
		get
		{
			return (bool)this["RememberPassword"];
		}
		set
		{
			this["RememberPassword"] = value;
		}
	}

	static Settings()
	{
		Settings.defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());
	}
}
