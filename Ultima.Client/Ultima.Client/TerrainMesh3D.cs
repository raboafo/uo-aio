using UOAIO;

namespace Ultima.Client;

public sealed class TerrainMesh3D : TerrainMesh
{
	private TerrainMeshProvider _meshProvider;

	private bool _foldLeftRight;

	private Point3D[] _normals;

	public TerrainMeshProvider Provider => this._meshProvider;

	public TerrainMesh3D(TerrainMeshProvider meshProvider)
	{
		this._meshProvider = meshProvider;
	}

	private unsafe static void UpdateSimpleMesh(TransformedColoredTextured* pMesh, int s00, int s10, int s11, int s01)
	{
		pMesh->Color = Renderer.GetQuadColor(65793 * s00);
		pMesh[1].Color = Renderer.GetQuadColor(65793 * s10);
		pMesh[2].Color = Renderer.GetQuadColor(65793 * s01);
		pMesh[3].Color = Renderer.GetQuadColor(65793 * s11);
	}

	private void AddNormals(Vector[] normals, int dx, int dy)
	{
	}

	private unsafe void UpdateMeshComplex(int s00, int s10, int s11, int s01)
	{
		int stride = this._meshProvider.Stride;
		int divisions = this._meshProvider.Divisions;
		int[] indices = this._meshProvider.GetIndices(this._foldLeftRight);
		fixed (int* ptr = indices)
		{
			int* ptr2 = ptr + indices.Length;
			fixed (TransformedColoredTextured* mesh = base._mesh)
			{
				TransformedColoredTextured* ptr3 = mesh;
				int num = 0;
				int num2 = divisions;
				while (num <= divisions)
				{
					int num3 = (s00 * num2 + s01 * num) / divisions;
					int num4 = (s10 * num2 + s11 * num) / divisions;
					int num5 = 0;
					int num6 = divisions;
					while (num5 <= divisions)
					{
						int num7 = (num3 * num6 + num4 * num5) / divisions;
						Point3D a = this._normals[num * stride + num5];
						int num8 = ((num5 == 0) ? (-1) : ((num5 == divisions) ? 1 : 0));
						int num9 = ((num == 0) ? (-1) : ((num == divisions) ? 1 : 0));
						int num10 = 65793;
						if (num8 != 0)
						{
							LandTile landTile = World.Viewport.GetLandTile(base.tile.ax + num8, base.tile.ay, Engine.m_World);
							if ((base.tile.m_Guarded && !base.tile.Impassable) != (landTile.m_Guarded && !landTile.Impassable))
							{
								num10 = ((base.tile.m_Guarded && !base.tile.Impassable) ? 256 : 65536);
							}
							landTile.EnsureMesh(landTile._lastMeshHue ?? base.tile._lastMeshHue ?? Hues.Default, this._meshProvider);
							if (landTile._mesh is TerrainMesh3D terrainMesh3D)
							{
								terrainMesh3D.Update(terrainMesh3D.tile, terrainMesh3D._color);
								a += terrainMesh3D._normals[num * stride + num6];
							}
						}
						if (num9 != 0)
						{
							LandTile landTile2 = World.Viewport.GetLandTile(base.tile.ax, base.tile.ay + num9, Engine.m_World);
							if ((base.tile.m_Guarded && !base.tile.Impassable) != (landTile2.m_Guarded && !landTile2.Impassable))
							{
								num10 = ((base.tile.m_Guarded && !base.tile.Impassable) ? 256 : 65536);
							}
							landTile2.EnsureMesh(landTile2._lastMeshHue ?? base.tile._lastMeshHue ?? Hues.Default, this._meshProvider);
							if (landTile2._mesh is TerrainMesh3D terrainMesh3D2)
							{
								terrainMesh3D2.Update(terrainMesh3D2.tile, terrainMesh3D2._color);
								a += terrainMesh3D2._normals[num2 * stride + num5];
							}
						}
						if (num8 != 0 && num9 != 0)
						{
							LandTile landTile3 = World.Viewport.GetLandTile(base.tile.ax + num8, base.tile.ay + num9, Engine.m_World);
							if ((base.tile.m_Guarded && !base.tile.Impassable) != (landTile3.m_Guarded && !landTile3.Impassable))
							{
								num10 = ((base.tile.m_Guarded && !base.tile.Impassable) ? 256 : 65536);
							}
							landTile3.EnsureMesh(landTile3._lastMeshHue ?? base.tile._lastMeshHue ?? Hues.Default, this._meshProvider);
							if (landTile3._mesh is TerrainMesh3D terrainMesh3D3)
							{
								terrainMesh3D3.Update(terrainMesh3D3.tile, terrainMesh3D3._color);
								a += terrainMesh3D3._normals[num2 * stride + num6];
							}
						}
						a = Point3D.Normalize256(a);
						num7 = (num7 * 2 + MapLighting.GetShadow(a) + 2) / 3;
						ptr3++->Color = Renderer.GetQuadColor(num10 * num7);
						num5++;
						num6--;
					}
					num++;
					num2--;
				}
			}
		}
	}

	protected unsafe override void UpdateMesh(LandTile tile, int baseColor)
	{
		int shadow = MapLighting.GetShadow(tile.facet, tile.ax, tile.ay);
		int shadow2 = MapLighting.GetShadow(tile.facet, tile.ax + 1, tile.ay);
		int shadow3 = MapLighting.GetShadow(tile.facet, tile.ax + 1, tile.ay + 1);
		int shadow4 = MapLighting.GetShadow(tile.facet, tile.ax, tile.ay + 1);
		fixed (TransformedColoredTextured* mesh = base._mesh)
		{
			TransformedColoredTextured* ptr = mesh;
			TransformedColoredTextured* ptr2 = mesh + base._mesh.Length;
			int divisions = this._meshProvider.Divisions;
			int stride = this._meshProvider.Stride;
			if (divisions == 1)
			{
				TerrainMesh3D.UpdateSimpleMesh(mesh, shadow, shadow2, shadow3, shadow4);
			}
			else
			{
				this.UpdateMeshComplex(shadow, shadow2, shadow3, shadow4);
			}
		}
	}

	private TransformedColoredTextured[] AllocateMesh(int size)
	{
		TransformedColoredTextured[] array = base._mesh;
		if (array == null || array.Length != size)
		{
			array = (base._mesh = new TransformedColoredTextured[size]);
		}
		return array;
	}

	private Point3D[] AllocateNormals(int size)
	{
		Point3D[] array = this._normals;
		if (array == null || array.Length != size)
		{
			array = (this._normals = new Point3D[size]);
		}
		return array;
	}

	private int[] GetIndices(out int indexCount)
	{
		int[] indices = this._meshProvider.GetIndices(this._foldLeftRight);
		indexCount = indices.Length;
		return indices;
	}

	private unsafe void ComputeMeshCore(float* pHeightmap, TransformedColoredTextured* pMesh, int divisions)
	{
		float num = 22f / (float)divisions;
		for (int i = 0; i <= divisions; i++)
		{
			float tv = (float)i / (float)divisions;
			for (int j = 0; j <= divisions; j++)
			{
				float tu = (float)j / (float)divisions;
				pMesh->Rhw = 1f;
				pMesh->Tu = tu;
				pMesh->Tv = tv;
				pMesh->X = 21.5f + (float)(j - i) * num;
				pMesh->Y = -0.5f + (float)(j + i) * num - *pHeightmap * 4f;
				pMesh++;
				pHeightmap++;
			}
		}
	}

	private unsafe void ComputeMesh(float* pHeightmap)
	{
		int size = this._meshProvider.Size;
		int divisions = this._meshProvider.Divisions;
		fixed (TransformedColoredTextured* pMesh = this.AllocateMesh(size))
		{
			this.ComputeMeshCore(pHeightmap, pMesh, divisions);
		}
	}

	private unsafe void ComputeNormals(float* pHeightmap)
	{
		int size = this._meshProvider.Size;
		int divisions = this._meshProvider.Divisions;
		Point3D* pVertices = stackalloc Point3D[size];
		this.ComputeVerticesCore(pHeightmap, pVertices, divisions);
		fixed (Point3D* pNormals = this.AllocateNormals(size))
		{
			int indexCount;
			fixed (int* indices = this.GetIndices(out indexCount))
			{
				MapLighting.AccumulateNormals(indices, pVertices, pNormals, indexCount, size);
			}
		}
	}

	private unsafe void ComputeVerticesCore(float* pHeightmap, Point3D* pVertices, int divisions)
	{
		for (int i = 0; i <= divisions; i++)
		{
			for (int j = 0; j <= divisions; j++)
			{
				pVertices->X = 22 * (j - i);
				pVertices->Y = 22 * (j + i);
				pVertices->Z = (int)((float)(4 * divisions) * *pHeightmap);
				pVertices++;
				pHeightmap++;
			}
		}
	}

	protected unsafe override TransformedColoredTextured[] GetMesh(LandTile tile, Texture tex)
	{
		int* ptr = stackalloc int[16];
		float* ptr2 = stackalloc float[this._meshProvider.Size];
		Point3D* ptr3 = stackalloc Point3D[this._meshProvider.Size];
		TileMatrix matrix = Map.GetMatrix(Engine.m_World);
		int* ptr4 = ptr;
		for (int i = -1; i < 3; i++)
		{
			for (int j = -1; j < 3; j++)
			{
				*(ptr4++) = matrix.GetLandTile(tile.ax + j, tile.ay + i).z;
			}
		}
		this._meshProvider.Sample(ptr, ptr2);
		this._foldLeftRight = tile.m_FoldLeftRight;
		this.ComputeMesh(ptr2);
		this.ComputeNormals(ptr2);
		return base._mesh;
	}

	protected unsafe override void RenderAux(TransformedColoredTextured* pMesh)
	{
		Renderer.FilterEnable = true;
		Renderer.PushVertices(pMesh, base._mesh.Length, this._foldLeftRight ? 2 : 3);
		Renderer.FilterEnable = false;
	}
}
