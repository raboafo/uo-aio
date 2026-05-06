using System;
using Ultima.Client;
using Ultima.Client.Terrain;
using Ultima.Data;

namespace UOAIO;

public sealed class LandTile : ITile, ICell, IDisposable
{
	private static readonly Type MyType;

	public sbyte z00;

	public sbyte z10;

	public sbyte z11;

	public sbyte z01;

	private Point3D? normal;

	public int facet;

	public int ax;

	public int ay;

	public int x;

	public int y;

	public int rev;

	public int graphicId;

	public ushort m_ID;

	public sbyte m_Z;

	private sbyte m_SortZ;

	public byte m_Height;

	public Texture m_sDraw;

	public bool m_Guarded;

	public bool m_FoldLeftRight;

	public IHue _lastMeshHue;

	public IMeshProvider _lastMeshProvider;

	public TerrainMesh _mesh;

	public LandId LandId
	{
		get
		{
			return (LandId)this.m_ID;
		}
		set
		{
			if ((int)value >= 0 && (int)value < 16384)
			{
				this.m_ID = (ushort)value;
			}
			else
			{
				this.m_ID = 580;
			}
			this.graphicId = GraphicTranslators.Art.Convert(this.m_ID);
		}
	}

	public unsafe TileFlag LandFlags => this.LandDataPointer->Flags;

	public unsafe LandData* LandDataPointer => Map.GetLandDataPointer(this.LandId);

	public bool Impassable => (this.LandFlags & TileFlag.Impassable) != 0;

	public bool Ignored => this.m_ID == 2 || this.m_ID == 475 || (this.m_ID >= 430 && this.m_ID <= 437);

	public Type CellType => LandTile.MyType;

	public ushort ID => this.m_ID;

	public sbyte Z => this.m_Z;

	public sbyte SortZ
	{
		get
		{
			return this.m_SortZ;
		}
		set
		{
			this.m_SortZ = value;
		}
	}

	public byte Height
	{
		get
		{
			return this.m_Height;
		}
		set
		{
			this.m_Height = value;
		}
	}

	void IDisposable.Dispose()
	{
	}

	private static Point3D Average4(Point3D a, Point3D b, Point3D c, Point3D d)
	{
		return new Point3D((a.X + b.X + c.X + d.X) / 4, (a.Y + b.Y + c.Y + d.Y) / 4, (a.Z + b.Z + c.Z + d.Z) / 4);
	}

	private static Point3D Average(Point3D a, Point3D b)
	{
		return new Point3D((a.X + b.X) / 2, (a.Y + b.Y) / 2, (a.Z + b.Z) / 2);
	}

	private Point3D ComputeNormal()
	{
		Point3D a = new Point3D(22 * (this.x - this.y), 22 * (this.x + this.y), 4 * this.z00);
		Point3D point3D = new Point3D(a.X + 22, a.Y + 22, 4 * this.z10);
		Point3D point3D2 = new Point3D(a.X, a.Y + 44, 4 * this.z11);
		Point3D c = new Point3D(a.X - 22, a.Y + 22, 4 * this.z01);
		Point3D a2;
		Point3D b;
		if (this.m_FoldLeftRight)
		{
			a2 = Point3D.Cross(a, point3D, point3D2);
			b = Point3D.Cross(a, point3D2, c);
		}
		else
		{
			a2 = Point3D.Cross(a, point3D, c);
			b = Point3D.Cross(point3D, point3D2, c);
		}
		return LandTile.Average(a2, b);
	}

	public Point3D GetNormal()
	{
		if (!this.normal.HasValue)
		{
			this.normal = this.ComputeNormal();
		}
		return this.normal.Value;
	}

	public static Point3D GetVertexNormal(int facet, int x, int y)
	{
		Viewport viewport = World.Viewport;
		Point3D a = viewport.GetLandTile(x - 1, y - 1, facet).GetNormal();
		Point3D b = viewport.GetLandTile(x, y - 1, facet).GetNormal();
		Point3D c = viewport.GetLandTile(x, y, facet).GetNormal();
		Point3D d = viewport.GetLandTile(x - 1, y, facet).GetNormal();
		return Point3D.Normalize256(LandTile.Average4(a, b, c, d));
	}

	public Point3D GetNormal(int xo, int yo)
	{
		return LandTile.GetVertexNormal(this.facet, this.ax + xo, this.ay + yo);
	}

	public void Prepare(int facet, int x, int y)
	{
		if (this.facet != facet || this.ax != x || this.ay != y)
		{
			TileMatrix matrix = Map.GetMatrix(facet);
			Tile landTile = matrix.GetLandTile(x, y);
			Tile landTile2 = matrix.GetLandTile(x, y + 1);
			Tile landTile3 = matrix.GetLandTile(x + 1, y);
			Tile landTile4 = matrix.GetLandTile(x + 1, y + 1);
			this.LandId = landTile.landId;
			this.facet = facet;
			this.ax = x;
			this.ay = y;
			this.z00 = landTile.z;
			this.z01 = landTile2.z;
			this.z11 = landTile4.z;
			this.z10 = landTile3.z;
			this.m_Z = (this.m_SortZ = this.z00);
			this.m_FoldLeftRight = Math.Abs(this.z00 - this.z11) <= Math.Abs(this.z01 - this.z10);
			if (this.m_FoldLeftRight)
			{
				this.m_SortZ = (sbyte)Map.FloorAverage(this.z00, this.z11);
			}
			else
			{
				this.m_SortZ = (sbyte)Map.FloorAverage(this.z01, this.z10);
			}
			this.m_Guarded = Region.Find(Region.GuardedRegions, x, y, this.z00, facet) != null;
			this.m_Height = 0;
			this.m_sDraw = null;
			this.normal = null;
			this._mesh = null;
			this.rev = -1;
		}
	}

	public void EnsureMesh(IHue hue, TerrainMeshProvider meshProvider)
	{
		if (this._mesh != null && this._lastMeshHue == hue && this._lastMeshProvider == meshProvider)
		{
			return;
		}
		this._lastMeshHue = hue;
		this._lastMeshProvider = meshProvider;
		int landId = this.graphicId;
		int texture = Map.GetTexture(landId);
		Texture texture2;
		if (texture > 0 && texture < 16384)
		{
			texture2 = hue.GetTerrainTexture(texture);
			if (texture2 == null || texture2.IsEmpty())
			{
				texture2 = hue.GetTerrainIsometric(landId);
				if (texture2 == null || texture2.IsEmpty())
				{
					texture2 = hue.GetTerrainTexture(1);
					if (texture2 == null || texture2.IsEmpty())
					{
						return;
					}
				}
			}
		}
		else
		{
			texture2 = hue.GetTerrainIsometric(landId);
			if (texture2 == null || texture2.IsEmpty())
			{
				if (texture > 0 && texture < 16384)
				{
					texture2 = hue.GetTerrainTexture(texture);
				}
				if (texture2 == null || texture2.IsEmpty())
				{
					texture2 = hue.GetTerrainTexture(1);
					if (texture2 == null || texture2.IsEmpty())
					{
						this._mesh = null;
						return;
					}
				}
			}
		}
		this.m_sDraw = texture2;
		if (texture2.Width == 44 && !(this._mesh is TerrainMesh2D))
		{
			this._mesh = new TerrainMesh2D();
			this._mesh.Load(this, texture2);
		}
		else if (texture2.Width != 44 && (!(this._mesh is TerrainMesh3D) || ((TerrainMesh3D)this._mesh).Provider != meshProvider))
		{
			this._mesh = new TerrainMesh3D(meshProvider);
			this._mesh.Load(this, texture2);
		}
	}

	static LandTile()
	{
		LandTile.MyType = typeof(LandTile);
	}
}
