namespace UOAIO;

public class UnpackLeaf
{
	public UnpackLeaf m_Left;

	public UnpackLeaf m_Right;

	public int[] m_Cache;

	public short m_Value = -1;

	public short m_Index;

	public UnpackLeaf(int index)
	{
		this.m_Index = (short)index;
	}
}
