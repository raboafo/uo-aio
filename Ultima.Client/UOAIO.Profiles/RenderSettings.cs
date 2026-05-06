using System.Reflection;
using Veritas;

namespace UOAIO.Profiles;

[Obfuscation(Feature = "renaming", ApplyToMembers = true)]
public class RenderSettings : PersistableObject
{
	public static readonly PersistableType TypeCode;

	private int _terrainQuality;

	private int _smoothingMode;

	private int _gameplayWindow;

	private bool _smoothCharacters;

	private bool _animatedCharacters;

	private bool _itemShadows;

	private bool _characterShadows;

	public override PersistableType TypeID => RenderSettings.TypeCode;

	public int TerrainQuality
	{
		get
		{
			return this._terrainQuality;
		}
		set
		{
			this._terrainQuality = value;
		}
	}

	public int SmoothingMode
	{
		get
		{
			return this._smoothingMode;
		}
		set
		{
			this._smoothingMode = value;
		}
	}

	public bool SmoothCharacters
	{
		get
		{
			return this._smoothCharacters;
		}
		set
		{
			this._smoothCharacters = value;
		}
	}

	public bool AnimatedCharacters
	{
		get
		{
			return this._animatedCharacters;
		}
		set
		{
			this._animatedCharacters = value;
		}
	}

	public bool CharacterShadows
	{
		get
		{
			return this._characterShadows;
		}
		set
		{
			this._characterShadows = value;
		}
	}

	public bool ItemShadows
	{
		get
		{
			return this._itemShadows;
		}
		set
		{
			this._itemShadows = value;
		}
	}

	public int GameplayWindow
	{
		get
		{
			return this._gameplayWindow;
		}
		set
		{
			this._gameplayWindow = value;
		}
	}

	private static PersistableObject Construct()
	{
		return new RenderSettings(isLoading: true);
	}

	public RenderSettings()
		: this(isLoading: false)
	{
	}

	private RenderSettings(bool isLoading)
	{
		if (!isLoading)
		{
			this._terrainQuality = 1;
			this._smoothingMode = 1;
			this._gameplayWindow = 1;
			this._smoothCharacters = true;
			this._animatedCharacters = true;
			this._itemShadows = true;
			this._characterShadows = true;
		}
	}

	protected override void SerializeAttributes(PersistanceWriter op)
	{
		op.SetInt32("terrain-quality", this._terrainQuality);
		op.SetInt32("smoothing-mode", this._smoothingMode);
		op.SetInt32("gameplay-window", this._gameplayWindow);
		op.SetBoolean("smooth-characters", this._smoothCharacters);
		op.SetBoolean("animated-characters", this._animatedCharacters);
		op.SetBoolean("item-shadows", this._itemShadows);
		op.SetBoolean("character-shadows", this._characterShadows);
	}

	protected override void DeserializeAttributes(PersistanceReader ip)
	{
		this._terrainQuality = ip.GetInt32("terrain-quality");
		this._smoothingMode = ip.GetInt32("smoothing-mode");
		this._gameplayWindow = ip.GetInt32("gameplay-window");
		this._smoothCharacters = ip.GetBoolean("smooth-characters");
		this._animatedCharacters = ip.GetBoolean("animated-characters");
		this._itemShadows = ip.GetBoolean("item-shadows");
		this._characterShadows = ip.GetBoolean("character-shadows");
	}

	static RenderSettings()
	{
		RenderSettings.TypeCode = new PersistableType("renderSettings", Construct);
	}
}
