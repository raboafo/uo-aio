namespace UOAIO;

public class GCombatGump : GDragable
{
	private GAbilityIcon m_PrimaryIcon;

	private GAbilityIcon m_SecondaryIcon;

	private static GCombatGump m_Instance;

	private static IHue m_ActiveHue;

	private static IHue m_AbleHue;

	private static IHue m_DefaultHue;

	public static void Open()
	{
		if (GCombatGump.m_Instance == null)
		{
			GCombatGump.m_Instance = new GCombatGump();
			Gumps.Desktop.Children.Add(GCombatGump.m_Instance);
		}
	}

	protected internal override void OnDispose()
	{
		base.OnDispose();
		GCombatGump.m_Instance = null;
	}

	public GCombatGump()
		: base(11010, 50, 50)
	{
		AbilityInfo[] abilities = AbilityInfo.Abilities;
		AbilityInfo active = AbilityInfo.Active;
		AbilityInfo primary = AbilityInfo.Primary;
		AbilityInfo secondary = AbilityInfo.Secondary;
		IFont uniFont = Engine.GetUniFont(1);
		OnClick onClick = Name_OnClick;
		GLabel toAdd = new GLabel("INDEX", Engine.GetFont(6), Hues.Default, 100, 4);
		base.m_Children.Add(toAdd);
		toAdd = new GLabel("INDEX", Engine.GetFont(6), Hues.Default, 262, 4);
		base.m_Children.Add(toAdd);
		for (int i = 0; i < abilities.Length; i++)
		{
			AbilityInfo abilityInfo = abilities[i];
			IHue hueFor = GCombatGump.GetHueFor(abilityInfo);
			toAdd = new GTextButton(Localization.GetString(abilityInfo.Name), uniFont, hueFor, hueFor, 56 + i / 9 * 162, 38 + i % 9 * 15, onClick);
			abilityInfo.NameLabel = (GTextButton)toAdd;
			toAdd.SetTag("Ability", abilityInfo);
			toAdd.Tooltip = new Tooltip(Localization.GetString(abilityInfo.Tooltip), Big: true, 240);
			toAdd.Tooltip.Delay = 0.25f;
			base.m_Children.Add(toAdd);
		}
		this.m_PrimaryIcon = new GAbilityIcon(inBook: true, primary: true, primary.Icon, 218, 105);
		this.m_PrimaryIcon.Tooltip = new Tooltip(Localization.GetString(primary.Name), Big: true);
		this.m_PrimaryIcon.Tooltip.Delay = 0.25f;
		this.m_PrimaryIcon.Hue = ((primary == AbilityInfo.Active) ? Hues.Load(32806) : Hues.Default);
		base.m_Children.Add(this.m_PrimaryIcon);
		toAdd = new GLabel("Primary", Engine.GetFont(6), Hues.Default, 268, 105);
		base.m_Children.Add(toAdd);
		toAdd = new GLabel("Ability Icon", Engine.GetFont(6), Hues.Default, 268, 119);
		base.m_Children.Add(toAdd);
		this.m_SecondaryIcon = new GAbilityIcon(inBook: true, primary: false, secondary.Icon, 218, 150);
		this.m_SecondaryIcon.Tooltip = new Tooltip(Localization.GetString(secondary.Name), Big: true);
		this.m_SecondaryIcon.Tooltip.Delay = 0.25f;
		this.m_SecondaryIcon.Hue = ((secondary == AbilityInfo.Active) ? Hues.Load(32806) : Hues.Default);
		base.m_Children.Add(this.m_SecondaryIcon);
		toAdd = new GLabel("Secondary", Engine.GetFont(6), Hues.Default, 268, 150);
		base.m_Children.Add(toAdd);
		toAdd = new GLabel("Ability Icon", Engine.GetFont(6), Hues.Default, 268, 164);
		base.m_Children.Add(toAdd);
	}

	private void InternalUpdate()
	{
		Gump[] array = base.m_Children.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] is GTextButton gTextButton)
			{
				AbilityInfo abilityInfo = (AbilityInfo)gTextButton.GetTag("Ability");
				if (abilityInfo != null)
				{
					GTextButton nameLabel = abilityInfo.NameLabel;
					IHue focusHue = (abilityInfo.NameLabel.DefaultHue = GCombatGump.GetHueFor(abilityInfo));
					nameLabel.FocusHue = focusHue;
				}
			}
		}
	}

	public static void Update()
	{
		if (GCombatGump.m_Instance != null)
		{
			GCombatGump.m_Instance.InternalUpdate();
		}
		GAbilityIcon.Update();
	}

	private void Name_OnClick(Gump sender)
	{
		AbilityInfo abilityInfo = (AbilityInfo)sender.GetTag("Ability");
		if (AbilityInfo.Active == abilityInfo)
		{
			AbilityInfo.Active = null;
		}
		else
		{
			AbilityInfo.Active = abilityInfo;
		}
	}

	public static IHue GetHueFor(AbilityInfo a)
	{
		if (a == AbilityInfo.Active)
		{
			return GCombatGump.m_ActiveHue;
		}
		if (a == AbilityInfo.Primary || a == AbilityInfo.Secondary)
		{
			return GCombatGump.m_AbleHue;
		}
		return GCombatGump.m_DefaultHue;
	}

	static GCombatGump()
	{
		GCombatGump.m_ActiveHue = new Hues.ColorFillHue(5903637);
		GCombatGump.m_AbleHue = new Hues.ColorFillHue(1381722);
		GCombatGump.m_DefaultHue = new Hues.ColorFillHue(5917233);
	}
}
