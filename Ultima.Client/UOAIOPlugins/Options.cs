namespace UOAIOPlugins;

public static class Options
{
	private static bool autoBandage;

	private static bool autoHeal;

	private static bool autoMeditate;

	private static bool autoMount;

	private static bool autoStrPot;

	private static bool autoStun;

	private static bool autoPopPouch;

	private static bool autoCure;

	private static bool autoLootCorpseNG;

	private static bool greatlyFinder;

	private static bool idocTimer;

	private static bool tameBot;

	private static bool pathFinding;

	private static bool dropbag;

	public static bool DropBag
	{
		get
		{
			return Options.dropbag;
		}
		set
		{
			Options.dropbag = value;
		}
	}

	public static bool PathFinding
	{
		get
		{
			return Options.pathFinding;
		}
		set
		{
			Options.pathFinding = value;
		}
	}

	public static bool AutoBandage
	{
		get
		{
			return Options.autoBandage;
		}
		set
		{
			Options.autoBandage = value;
		}
	}

	public static bool AutoHeal
	{
		get
		{
			return Options.autoHeal;
		}
		set
		{
			Options.autoHeal = value;
		}
	}

	public static bool AutoMount
	{
		get
		{
			return Options.autoMount;
		}
		set
		{
			Options.autoMount = value;
		}
	}

	public static bool AutoMeditate
	{
		get
		{
			return Options.autoMeditate;
		}
		set
		{
			Options.autoMeditate = value;
		}
	}

	public static bool AutoStrPot
	{
		get
		{
			return Options.autoStrPot;
		}
		set
		{
			Options.autoStrPot = value;
		}
	}

	public static bool AutoStun
	{
		get
		{
			return Options.autoStun;
		}
		set
		{
			Options.autoStun = value;
		}
	}

	public static bool AutoPopPouch
	{
		get
		{
			return Options.autoPopPouch;
		}
		set
		{
			Options.autoPopPouch = value;
		}
	}

	public static bool AutoCure
	{
		get
		{
			return Options.autoCure;
		}
		set
		{
			Options.autoCure = value;
		}
	}

	public static bool AutoLootCorpseNG
	{
		get
		{
			return Options.autoLootCorpseNG;
		}
		set
		{
			Options.autoLootCorpseNG = value;
		}
	}

	public static bool GreatlyFinder
	{
		get
		{
			return Options.greatlyFinder;
		}
		set
		{
			Options.greatlyFinder = value;
		}
	}

	public static bool IdocTimer
	{
		get
		{
			return Options.idocTimer;
		}
		set
		{
			Options.idocTimer = value;
		}
	}

	public static bool TameBot
	{
		get
		{
			return Options.tameBot;
		}
		set
		{
			Options.tameBot = value;
		}
	}
}
