using System;

namespace UOAIO;

public class DesignerArrowButton : GButtonNew
{
	private DesignerGump m_Designer;

	private int m_Direction;

	public int Direction
	{
		get
		{
			return this.m_Direction;
		}
		set
		{
			this.m_Direction = value;
		}
	}

	public DesignerArrowButton(DesignerGump designer, int direction, int x, int y, int inactiveID, int activeID, int pressedID)
		: base(inactiveID, activeID, pressedID, x, y)
	{
		this.m_Designer = designer;
		this.m_Direction = direction;
	}

	protected override void OnClicked()
	{
		DesignerGroup designerGroup = this.m_Designer.Group;
		if (designerGroup == null)
		{
			return;
		}
		DesignerGroup parent = this.m_Designer.Group.Parent;
		if (parent == null || !parent.UseArrows)
		{
			return;
		}
		if (this.m_Direction == 0)
		{
			this.m_Designer.Group = parent.Parent;
			return;
		}
		int num = Array.IndexOf(parent.Groups, designerGroup);
		if (num != -1)
		{
			num += this.m_Direction;
			num = ((num >= 0) ? (num % parent.Groups.Length) : (num + parent.Groups.Length));
			this.m_Designer.Group = parent.Groups[num];
		}
	}
}
