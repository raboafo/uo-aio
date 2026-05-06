using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Ultima.Client.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (Resources.resourceMan == null)
			{
				ResourceManager resourceManager = new ResourceManager("Ultima.Client.Properties.Resources", typeof(Resources).Assembly);
				Resources.resourceMan = resourceManager;
			}
			return Resources.resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return Resources.resourceCulture;
		}
		set
		{
			Resources.resourceCulture = value;
		}
	}

	internal static Bitmap discord
	{
		get
		{
			object obj = Resources.ResourceManager.GetObject("discord", Resources.resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap Exit
	{
		get
		{
			object obj = Resources.ResourceManager.GetObject("Exit", Resources.resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap exit_pushed
	{
		get
		{
			object obj = Resources.ResourceManager.GetObject("exit_pushed", Resources.resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap Login
	{
		get
		{
			object obj = Resources.ResourceManager.GetObject("Login", Resources.resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap Login_PUSHED
	{
		get
		{
			object obj = Resources.ResourceManager.GetObject("Login_PUSHED", Resources.resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal static Bitmap veritas_bg
	{
		get
		{
			object obj = Resources.ResourceManager.GetObject("veritas_bg", Resources.resourceCulture);
			return (Bitmap)obj;
		}
	}

	internal Resources()
	{
	}
}
