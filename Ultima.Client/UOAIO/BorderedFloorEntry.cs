namespace UOAIO;

public class BorderedFloorEntry : DesignerEntry
{
	private DesignerID[] m_IDs;

	public BorderedFloorEntry(params DesignerID[] ids)
		: base(ids[0])
	{
		this.m_IDs = ids;
	}

	public override void FillCursor(DesignerID[] ids)
	{
		for (int i = 0; i < ids.Length; i++)
		{
			ids[i] = this.m_IDs[i];
		}
	}

	public override Multi GetMultiCursor()
	{
		return null;
	}
}
