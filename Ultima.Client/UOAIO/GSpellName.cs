namespace UOAIO;

public class GSpellName : GTextButton
{
	private int m_SpellID;

	public GSpellName(int SpellID, string Name, IFont Font, IHue HRegular, IHue HOver, int X, int Y)
		: base(Name, Font, HRegular, HOver, X, Y, null)
	{
		this.m_SpellID = SpellID;
		base.m_CanDrag = true;
		base.m_QuickDrag = false;
	}

	protected internal override void OnDragStart()
	{
		base.m_IsDragging = false;
		Gumps.Drag = null;
		GSpellIcon gSpellIcon = new GSpellIcon(this.m_SpellID);
		gSpellIcon.m_OffsetX = gSpellIcon.Width / 2;
		gSpellIcon.m_OffsetY = gSpellIcon.Height / 2;
		gSpellIcon.X = Engine.m_xMouse - gSpellIcon.m_OffsetX;
		gSpellIcon.Y = Engine.m_yMouse - gSpellIcon.m_OffsetY;
		gSpellIcon.m_IsDragging = true;
		Gumps.Desktop.Children.Add(gSpellIcon);
		Gumps.Drag = gSpellIcon;
	}

	protected internal override void OnDoubleClick(int X, int Y)
	{
		Spells.GetSpellByID(this.m_SpellID)?.Cast();
		Item item = (Item)base.m_Parent.GetTag("Container");
		item.LastSpell = this.m_SpellID;
		base.m_Parent.Visible = false;
		Gumps.Desktop.Children.Add(new GSpellbookIcon(base.m_Parent, item));
	}
}
