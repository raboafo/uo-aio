using System.Collections;
using System.Windows.Forms;

namespace UOAIO;

public class GRadioButton : GImage
{
	protected bool m_State;

	protected int[] m_GumpIDs;

	protected int m_Group;

	protected Gump m_ParentOverride;

	public Gump ParentOverride
	{
		get
		{
			return this.m_ParentOverride;
		}
		set
		{
			this.m_ParentOverride = value;
		}
	}

	public bool State
	{
		get
		{
			return this.m_State;
		}
		set
		{
			if (this.m_State == value)
			{
				return;
			}
			this.m_State = value;
			base.GumpID = this.m_GumpIDs[value ? 1 : 0];
			if (!value || (base.m_Parent == null && this.m_ParentOverride == null))
			{
				return;
			}
			Stack stack = new Stack();
			stack.Push((this.m_ParentOverride != null) ? this.m_ParentOverride : base.m_Parent);
			while (stack.Count > 0)
			{
				Gump gump = (Gump)stack.Pop();
				Gump[] array = gump.Children.ToArray();
				for (int i = 0; i < array.Length; i++)
				{
					gump = array[i];
					if (gump is GRadioButton && gump != this)
					{
						GRadioButton gRadioButton = (GRadioButton)gump;
						if (gRadioButton.m_Group == this.m_Group && gRadioButton.State)
						{
							gRadioButton.State = false;
						}
					}
					if (gump.Children.Count > 0)
					{
						stack.Push(gump);
					}
				}
			}
		}
	}

	public GRadioButton(int inactiveID, int activeID, bool initialState, int x, int y, int group)
		: base(initialState ? activeID : inactiveID, x, y)
	{
		this.m_GumpIDs = new int[2] { inactiveID, activeID };
		this.m_State = initialState;
		this.m_Group = group;
	}

	protected internal override void OnMouseUp(int x, int y, MouseButtons mb)
	{
		this.State = true;
	}

	protected internal override bool HitTest(int x, int y)
	{
		if (base.m_Invalidated)
		{
			base.Refresh();
		}
		return base.m_Draw && (base.m_Clipper == null || base.m_Clipper.Evaluate(base.PointToScreen(new Point(x, y)))) && base.m_Image.HitTest(x, y);
	}
}
