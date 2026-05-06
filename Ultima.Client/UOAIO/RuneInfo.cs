using System;
using Veritas;

namespace UOAIO;

public class RuneInfo : PersistableObject, IEquatable<RuneInfo>
{
	public static readonly PersistableType TypeCode;

	private string m_Name;

	private Point3D m_Point;

	private int m_Facet;

	public override PersistableType TypeID => RuneInfo.TypeCode;

	public string Name => this.m_Name;

	public Point3D Point => this.m_Point;

	public int Facet => this.m_Facet;

	private static PersistableObject Construct()
	{
		return new RuneInfo();
	}

	private RuneInfo()
	{
	}

	public RuneInfo(string name, Point3D p, int f)
	{
		this.m_Name = name;
		this.m_Point = p;
		this.m_Facet = f;
	}

	protected override void SerializeAttributes(PersistanceWriter op)
	{
		op.SetString("name", this.m_Name);
		op.SetInt32("point-x", this.m_Point.X);
		op.SetInt32("point-y", this.m_Point.Y);
		if (this.m_Point.Z != 0)
		{
			op.SetInt32("point-z", this.m_Point.Z);
		}
		if (this.m_Facet != 0)
		{
			op.SetInt32("point-f", this.m_Facet);
		}
	}

	protected override void DeserializeAttributes(PersistanceReader ip)
	{
		this.m_Name = ip.GetString("name");
		this.m_Point = new Point3D(ip.GetInt32("point-x"), ip.GetInt32("point-y"), ip.GetInt32("point-z"));
		this.m_Facet = ip.GetInt32("point-f");
	}

	public bool Equals(RuneInfo other)
	{
		return other != null && this.m_Name == other.m_Name && this.m_Point == other.m_Point && this.m_Facet == other.m_Facet;
	}

	public override int GetHashCode()
	{
		return this.m_Name.GetHashCode() ^ this.m_Point.GetHashCode() ^ this.m_Facet.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		return this.Equals(obj as RuneInfo);
	}

	static RuneInfo()
	{
		RuneInfo.TypeCode = new PersistableType("rune", Construct);
	}
}
