namespace UOAIO;

public sealed class DesignerGroup
{
	private int m_ContentOffsetX;

	private int m_ContentOffsetY;

	private int m_ContentOffsetWidth;

	private int m_ContentOffsetHeight;

	private bool m_QuickUse;

	private bool m_UseArrows;

	private DesignerGroup m_Parent;

	private DesignerGroup[] m_Groups;

	private DesignerEntry[] m_Entries;

	private DesignerID m_ID;

	public int ContentOffsetX
	{
		get
		{
			return this.m_ContentOffsetX;
		}
		set
		{
			this.m_ContentOffsetX = value;
		}
	}

	public int ContentOffsetY
	{
		get
		{
			return this.m_ContentOffsetY;
		}
		set
		{
			this.m_ContentOffsetY = value;
		}
	}

	public int ContentOffsetWidth
	{
		get
		{
			return this.m_ContentOffsetWidth;
		}
		set
		{
			this.m_ContentOffsetWidth = value;
		}
	}

	public int ContentOffsetHeight
	{
		get
		{
			return this.m_ContentOffsetHeight;
		}
		set
		{
			this.m_ContentOffsetHeight = value;
		}
	}

	public bool QuickUse
	{
		get
		{
			return this.m_QuickUse;
		}
		set
		{
			this.m_QuickUse = value;
		}
	}

	public bool UseArrows
	{
		get
		{
			return this.m_UseArrows;
		}
		set
		{
			this.m_UseArrows = value;
		}
	}

	public DesignerGroup Parent => this.m_Parent;

	public DesignerGroup[] Groups => this.m_Groups;

	public DesignerEntry[] Entries => this.m_Entries;

	public DesignerID ID => this.m_ID;

	public DesignerGroup(DesignerID id)
		: this(id, new DesignerGroup[0], new DesignerEntry[0])
	{
	}

	public DesignerGroup(DesignerID id, params DesignerGroup[] groups)
		: this(id, groups, new DesignerEntry[0])
	{
	}

	public DesignerGroup(DesignerID id, params DesignerEntry[] entries)
		: this(id, new DesignerGroup[0], entries)
	{
	}

	public DesignerGroup(DesignerID id, DesignerGroup[] groups, DesignerEntry[] entries)
	{
		this.m_ID = id;
		this.m_Groups = groups;
		this.m_Entries = entries;
	}

	public DesignerGroup AddGroup(DesignerID id)
	{
		DesignerGroup designerGroup = new DesignerGroup(id);
		this.AddGroup(designerGroup);
		return designerGroup;
	}

	public void AddGroup(DesignerGroup group)
	{
		DesignerGroup[] groups = this.m_Groups;
		this.m_Groups = new DesignerGroup[groups.Length + 1];
		for (int i = 0; i < groups.Length; i++)
		{
			this.m_Groups[i] = groups[i];
		}
		this.m_Groups[groups.Length] = group;
		group.m_ContentOffsetX = this.m_ContentOffsetX;
		group.m_ContentOffsetY = this.m_ContentOffsetY;
		group.m_ContentOffsetWidth = this.m_ContentOffsetWidth;
		group.m_ContentOffsetHeight = this.m_ContentOffsetHeight;
		group.m_Parent = this;
	}

	public void AddEntry(DesignerEntry entry)
	{
		DesignerEntry[] entries = this.m_Entries;
		this.m_Entries = new DesignerEntry[entries.Length + 1];
		for (int i = 0; i < entries.Length; i++)
		{
			this.m_Entries[i] = entries[i];
		}
		this.m_Entries[entries.Length] = entry;
	}

	public void AddBorderedFloorSet(params DesignerID[] ids)
	{
		DesignerGroup designerGroup = new DesignerGroup(ids[0]);
		designerGroup.AddEntry(new BorderedFloorEntry(ids));
		designerGroup.AddEntry(null);
		designerGroup.AddEntry(new SingleEntry(ids[0]));
		designerGroup.AddEntry(null);
		for (int i = 9; i < ids.Length; i++)
		{
			designerGroup.AddEntry(new SingleEntry(ids[i]));
		}
		for (int j = 0; j < 4 - (ids.Length - 9); j++)
		{
			designerGroup.AddEntry(null);
		}
		for (int k = 1; k < 9; k++)
		{
			designerGroup.AddEntry(new SingleEntry(ids[k]));
		}
		designerGroup.QuickUse = true;
		this.AddGroup(designerGroup);
	}

	public void AddTiledFloorSet(DesignerID id)
	{
		this.AddEntry(new SingleEntry(id));
	}

	public void AddIndexedFloorSet(params DesignerID[] ids)
	{
		DesignerGroup designerGroup = new DesignerGroup(ids[0]);
		for (int i = 0; i < ids.Length; i++)
		{
			designerGroup.AddEntry(new SingleEntry(ids[i]));
		}
		this.AddGroup(designerGroup);
	}
}
