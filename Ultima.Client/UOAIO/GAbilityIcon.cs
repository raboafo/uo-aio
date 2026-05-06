using System.Collections;

namespace UOAIO;

public class GAbilityIcon : GDragable
{
	private bool m_InBook;

	private bool m_Primary;

	private static ArrayList m_Instances;

	public static void Update()
	{
		for (int i = 0; i < GAbilityIcon.m_Instances.Count; i++)
		{
			GAbilityIcon gAbilityIcon = (GAbilityIcon)GAbilityIcon.m_Instances[i];
			AbilityInfo abilityInfo = (gAbilityIcon.m_Primary ? AbilityInfo.Primary : AbilityInfo.Secondary);
			gAbilityIcon.GumpID = abilityInfo.Icon;
			gAbilityIcon.Hue = ((abilityInfo == AbilityInfo.Active) ? Hues.Load(32806) : Hues.Default);
			gAbilityIcon.Tooltip = new Tooltip(Localization.GetString(abilityInfo.Name), Big: true);
			gAbilityIcon.Tooltip.Delay = 0.25f;
		}
	}

	public GAbilityIcon(bool inBook, bool primary, int gumpID, int x, int y)
		: base(gumpID, x, y)
	{
		this.m_InBook = inBook;
		this.m_Primary = primary;
		GAbilityIcon.m_Instances.Add(this);
		base.m_QuickDrag = false;
		base.m_CanClose = !inBook;
	}

	protected internal override void OnDispose()
	{
		GAbilityIcon.m_Instances.Remove(this);
		base.OnDispose();
	}

	protected internal override void OnDragStart()
	{
		if (this.m_InBook)
		{
			base.m_IsDragging = false;
			Gumps.Drag = null;
			GAbilityIcon gAbilityIcon = new GAbilityIcon(inBook: false, this.m_Primary, base.GumpID, Engine.m_xMouse, Engine.m_yMouse);
			gAbilityIcon.Hue = base.Hue;
			gAbilityIcon.m_OffsetX = gAbilityIcon.Width / 2;
			gAbilityIcon.m_OffsetY = gAbilityIcon.Height / 2;
			gAbilityIcon.X = Engine.m_xMouse - gAbilityIcon.m_OffsetX;
			gAbilityIcon.Y = Engine.m_yMouse - gAbilityIcon.m_OffsetY;
			gAbilityIcon.m_IsDragging = true;
			Gumps.Desktop.Children.Add(gAbilityIcon);
			Gumps.Drag = gAbilityIcon;
		}
		else
		{
			base.OnDragStart();
		}
	}

	protected internal override void OnDoubleClick(int X, int Y)
	{
		AbilityInfo abilityInfo = (this.m_Primary ? AbilityInfo.Primary : AbilityInfo.Secondary);
		if (AbilityInfo.Active == abilityInfo)
		{
			AbilityInfo.Active = null;
		}
		else
		{
			AbilityInfo.Active = abilityInfo;
		}
	}

	static GAbilityIcon()
	{
		GAbilityIcon.m_Instances = new ArrayList();
	}
}
