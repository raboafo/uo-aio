namespace UOAIO;

public class RestoreInfo
{
	public int m_X;

	public int m_Y;

	public int m_Z;

	public Agent m_Parent;

	public RestoreInfo(Item item)
	{
		this.m_X = item.X;
		this.m_Y = item.Y;
		this.m_Z = item.Z;
		this.m_Parent = item.Parent;
	}
}
