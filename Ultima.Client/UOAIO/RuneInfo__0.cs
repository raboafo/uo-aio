using System;
using Veritas;

namespace UOAIO;

public class RuneInfo__0 : PersistableObject, IEquatable<RuneInfo__0>
{
	public static readonly PersistableType TypeCode;

	private string m_Name;

	private Point3D m_Point;

	private int m_Facet;

	public override PersistableType TypeID => RuneInfo__0.TypeCode;

	public string Name => this.m_Name;

	public Point3D Point => this.m_Point;

	public int Facet => this.m_Facet;

	private static PersistableObject Construct()
	{
		return new RuneInfo__0();
	}

	private RuneInfo__0()
	{
	}

	public RuneInfo__0(string name, Point3D p, int f)
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

	public bool Equals(RuneInfo__0 other)
	{
		if (other != null && this.m_Name == other.m_Name && this.m_Point == other.m_Point)
		{
			return this.m_Facet == other.m_Facet;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return this.m_Name.GetHashCode() ^ this.m_Point.GetHashCode() ^ this.m_Facet.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		return this.Equals(obj as RuneInfo__0);
	}

	static RuneInfo__0()
	{
		RuneInfo__0.TypeCode = new PersistableType("rune", Construct);
	}
}
