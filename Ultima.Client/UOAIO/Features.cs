namespace UOAIO;

public class Features
{
	private bool m_Chat;

	private bool m_LBR;

	private bool m_AOS;

	public bool Chat
	{
		get
		{
			return this.m_Chat;
		}
		set
		{
			this.m_Chat = value;
		}
	}

	public bool LBR
	{
		get
		{
			return this.m_LBR;
		}
		set
		{
			this.m_LBR = value;
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
}
