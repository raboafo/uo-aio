namespace UOAIO;

public class ServerFeatures
{
	private bool m_ContextMenus;

	private bool m_SingleChar;

	private bool m_AOS;

	public bool ContextMenus
	{
		get
		{
			return this.m_ContextMenus;
		}
		set
		{
			this.m_ContextMenus = value;
		}
	}

	public bool SingleChar
	{
		get
		{
			return this.m_SingleChar;
		}
		set
		{
			this.m_SingleChar = value;
		}
	}

	public bool AOS
	{
		get
		{
			return this.m_AOS;
		}
		set
		{
			this.m_AOS = value;
		}
	}

	public ServerFeatures()
	{
		this.m_ContextMenus = false;
		this.m_SingleChar = false;
		this.m_AOS = false;
	}
}
