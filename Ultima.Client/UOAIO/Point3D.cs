using System;

namespace UOAIO;

public struct Point3D : IEquatable<Point3D>
{
	public int X;

	public int Y;

	public int Z;

	public Point3D(int x, int y, int z)
	{
		this.X = x;
		this.Y = y;
		this.Z = z;
	}

	public Point3D(IPoint3D p)
	{
		this.X = p.X;
		this.Y = p.Y;
		this.Z = p.Z;
	}

	public override bool Equals(object o)
	{
		if (o == null || !(o is IPoint3D))
		{
			return false;
		}
		IPoint3D point3D = (IPoint3D)o;
		return this.X == point3D.X && this.Y == point3D.Y && this.Z == point3D.Z;
	}

	public override int GetHashCode()
	{
		return this.X ^ this.Y ^ this.Z;
	}

	public static Point3D Parse(string value)
	{
		int num = value.IndexOf('(');
		int num2 = value.IndexOf(',', num + 1);
		string value2 = value.Substring(num + 1, num2 - (num + 1)).Trim();
		num = num2;
		num2 = value.IndexOf(',', num + 1);
		string value3 = value.Substring(num + 1, num2 - (num + 1)).Trim();
		num = num2;
		num2 = value.IndexOf(')', num + 1);
		string value4 = value.Substring(num + 1, num2 - (num + 1)).Trim();
		return new Point3D(Convert.ToInt32(value2), Convert.ToInt32(value3), Convert.ToInt32(value4));
	}

	public static bool operator ==(Point3D l, Point3D r)
	{
		return l.X == r.X && l.Y == r.Y && l.Z == r.Z;
	}

	public static bool operator !=(Point3D l, Point3D r)
	{
		return l.X != r.X || l.Y != r.Y || l.Z != r.Z;
	}

	public static bool operator ==(Point3D l, IPoint3D r)
	{
		return l.X == r.X && l.Y == r.Y && l.Z == r.Z;
	}

	public static bool operator !=(Point3D l, IPoint3D r)
	{
		return l.X != r.X || l.Y != r.Y || l.Z != r.Z;
	}

	public static Point3D operator +(Point3D lhs, Point3D rhs)
	{
		return new Point3D(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z);
	}

	public bool Equals(Point3D other)
	{
		return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
	}

	public static Point3D Cross(Point3D a, Point3D b, Point3D c)
	{
		return new Point3D((c.Z - a.Z) * (b.Y - a.Y) - (c.Y - a.Y) * (b.Z - a.Z), (b.Z - a.Z) * (c.X - a.X) - (c.Z - a.Z) * (b.X - a.X), (c.Y - a.Y) * (b.X - a.X) - (b.Y - a.Y) * (c.X - a.X));
	}

	public static Point3D Normalize256(Point3D a)
	{
		double num = Math.Sqrt(a.X * a.X + a.Y * a.Y + a.Z * a.Z);
		double num2 = 256.0 / num;
		return new Point3D((int)((double)a.X * num2), (int)((double)a.Y * num2), (int)((double)a.Z * num2));
	}
}
