namespace UOAIO;

public class WrapKey
{
	public string m_ToWrap;

	public int m_MaxWidth;

	public int m_HashCode;

	public WrapKey(string toWrap, int maxWidth)
	{
		this.m_ToWrap = toWrap;
		this.m_MaxWidth = maxWidth;
		this.m_HashCode = (toWrap?.GetHashCode() ?? 0) ^ maxWidth;
	}

	public override int GetHashCode()
	{
		return this.m_HashCode;
	}

	public override bool Equals(object x)
	{
		WrapKey wrapKey = (WrapKey)x;
		return this == wrapKey || (this.m_HashCode == wrapKey.m_HashCode && this.m_MaxWidth == wrapKey.m_MaxWidth && this.m_ToWrap == wrapKey.m_ToWrap);
	}
}
