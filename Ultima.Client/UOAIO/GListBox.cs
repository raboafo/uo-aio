using System;

namespace UOAIO;

public class GListBox : GBackground
{
	protected IFont m_Font;

	protected IHue m_HRegular;

	protected IHue m_HOver;

	protected int m_Index;

	protected int m_ItemCount;

	protected int m_Count;

	protected OnClick m_OnClick;

	private static Type tGListItem;

	public OnClick OnClick
	{
		get
		{
			return this.m_OnClick;
		}
		set
		{
			this.m_OnClick = value;
		}
	}

	public int Count => this.m_Count;

	public int StartIndex
	{
		get
		{
			return this.m_Index;
		}
		set
		{
			if (this.m_Index == value)
			{
				return;
			}
			this.m_Index = value;
			Gump[] array = base.m_Children.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				Gump gump = array[i];
				Type type = gump.GetType();
				if (type == GListBox.tGListItem)
				{
					((GListItem)base.m_Children[i]).Layout();
				}
			}
		}
	}

	public int ItemCount => this.m_ItemCount;

	public IFont Font => this.m_Font;

	public IHue HRegular => this.m_HRegular;

	public IHue HOver => this.m_HOver;

	protected internal void OnListItemClick(GListItem who)
	{
		if (this.m_OnClick != null)
		{
			base.SetTag("Clicked", who);
			this.m_OnClick(this);
		}
	}

	public GListBox(IFont Font, IHue HRegular, IHue HOver, int BackID, int X, int Y, int Width, int Height, bool HasBorder)
		: base(BackID, Width, Height, X, Y, HasBorder)
	{
		this.m_Font = Font;
		this.m_HRegular = HRegular;
		this.m_HOver = HOver;
		this.m_ItemCount = base.UseHeight / 18;
	}

	public void AddItem(string Text)
	{
		base.m_Children.Add(new GListItem(Text, this.m_Count++, this));
	}

	public void AddItem(string Text, Tooltip t)
	{
		GListItem gListItem = new GListItem(Text, this.m_Count++, this);
		gListItem.Tooltip = t;
		base.m_Children.Add(gListItem);
	}

	static GListBox()
	{
		GListBox.tGListItem = typeof(GListItem);
	}
}
