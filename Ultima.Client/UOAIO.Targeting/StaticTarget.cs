namespace UOAIO.Targeting;

public class StaticTarget : IPoint3D, IPoint2D
{
	private int m_X;

	private int m_Y;

	private int m_Z;

	private int m_ID;

	private int m_RealID;

	private IHue m_Hue;

	public int X => this.m_X;

	public int Y => this.m_Y;

	public int Z => this.m_Z;

	public int ID => this.m_ID;

	public int RealID => this.m_RealID;

	public IHue Hue => this.m_Hue;

	public StaticTarget(int x, int y, int z, int id, int realID, IHue hue)
	{
		this.m_X = x;
		this.m_Y = y;
		this.m_Z = z;
		this.m_ID = id;
		this.m_RealID = realID;
		this.m_Hue = hue;
	}
}
