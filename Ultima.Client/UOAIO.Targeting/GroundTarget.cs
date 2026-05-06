namespace UOAIO.Targeting;

public class GroundTarget : IPoint3D, IPoint2D
{
	private int m_X;

	private int m_Y;

	private int m_Z;

	public int X => this.m_X;

	public int Y => this.m_Y;

	public int Z => this.m_Z;

	public GroundTarget(int x, int y, int z)
	{
		this.m_X = x;
		this.m_Y = y;
		this.m_Z = z;
	}
}
