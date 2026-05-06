namespace UOAIO;

public class DesignerLevelButton : GButtonNew
{
	private DesignContext m_Context;

	private int[] m_AllGumpIDs;

	private int m_Level;

	public int Level
	{
		get
		{
			return this.m_Level;
		}
		set
		{
			this.m_Level = value;
		}
	}

	public void Activate()
	{
		base.m_GumpID = new int[3]
		{
			this.m_AllGumpIDs[3],
			this.m_AllGumpIDs[4],
			this.m_AllGumpIDs[5]
		};
		this.Refresh();
	}

	public void Deactivate()
	{
		base.m_GumpID = new int[3]
		{
			this.m_AllGumpIDs[0],
			this.m_AllGumpIDs[1],
			this.m_AllGumpIDs[2]
		};
		this.Refresh();
	}

	public DesignerLevelButton(DesignContext context, int level, int x, int y, params int[] gumpIDs)
		: base(gumpIDs[0], gumpIDs[1], gumpIDs[2], x, y)
	{
		this.m_Context = context;
		this.m_AllGumpIDs = gumpIDs;
		this.m_Level = level;
	}

	protected override void OnClicked()
	{
		Network.Send(new PDesigner_Level(this.m_Context.House, this.m_Level));
	}
}
