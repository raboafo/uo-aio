namespace UOAIO;

public class Point3DList
{
	private Point3D[] m_List;

	private int m_Count;

	private static Point3D[] m_EmptyList;

	public int Count => this.m_Count;

	public Point3D Last => this.m_List[this.m_Count - 1];

	public Point3D this[int index] => this.m_List[index];

	public Point3DList()
	{
		this.m_List = new Point3D[8];
		this.m_Count = 0;
	}

	public void Clear()
	{
		this.m_Count = 0;
	}

	public void Add(int x, int y, int z)
	{
		if (this.m_Count + 1 > this.m_List.Length)
		{
			Point3D[] list = this.m_List;
			this.m_List = new Point3D[list.Length * 2];
			for (int i = 0; i < list.Length; i++)
			{
				this.m_List[i] = list[i];
			}
		}
		this.m_List[this.m_Count].X = x;
		this.m_List[this.m_Count].Y = y;
		this.m_List[this.m_Count].Z = z;
		this.m_Count++;
	}

	public void Add(Point3D p)
	{
		if (this.m_Count + 1 > this.m_List.Length)
		{
			Point3D[] list = this.m_List;
			this.m_List = new Point3D[list.Length * 2];
			for (int i = 0; i < list.Length; i++)
			{
				this.m_List[i] = list[i];
			}
		}
		this.m_List[this.m_Count].X = p.X;
		this.m_List[this.m_Count].Y = p.Y;
		this.m_List[this.m_Count].Z = p.Z;
		this.m_Count++;
	}

	public Point3D[] ToArray()
	{
		if (this.m_Count == 0)
		{
			return Point3DList.m_EmptyList;
		}
		Point3D[] array = new Point3D[this.m_Count];
		for (int i = 0; i < this.m_Count; i++)
		{
			array[i] = this.m_List[i];
		}
		this.m_Count = 0;
		return array;
	}

	static Point3DList()
	{
		Point3DList.m_EmptyList = new Point3D[0];
	}
}
