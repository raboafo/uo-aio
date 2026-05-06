namespace UOAIO;

public class SingleEntry : DesignerEntry
{
	private DesignerID m_ID;

	public SingleEntry(DesignerID id)
		: base(id)
	{
		this.m_ID = id;
	}

	public override void FillCursor(DesignerID[] ids)
	{
		for (int i = 0; i < ids.Length; i++)
		{
			ids[i] = this.m_ID;
		}
	}

	public override Multi GetMultiCursor()
	{
		return null;
	}
}
