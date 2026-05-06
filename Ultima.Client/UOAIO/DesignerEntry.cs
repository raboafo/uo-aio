namespace UOAIO;

public abstract class DesignerEntry
{
	private DesignerID m_ID;

	public DesignerID ID => this.m_ID;

	public DesignerEntry(DesignerID id)
	{
		this.m_ID = id;
	}

	public abstract void FillCursor(DesignerID[] ids);

	public abstract Multi GetMultiCursor();
}
