using System;

namespace UOAIO;

public static class MapLighting
{
	private const int SunX = 100;

	private const int SunY = -30;

	private const int SunZ = 23;

	public static bool[] m_AlwaysStretch;

	public static int GetShadow(Point3D normal)
	{
		int val = (normal.X * 100 + normal.Y * -30 + normal.Z * 23) / 1000;
		int num = Math.Min(Math.Max(0, val), 15);
		return 255 - num * 9;
	}

	public static int GetShadow(int facet, int x, int y)
	{
		Point3D vertexNormal = LandTile.GetVertexNormal(facet, x, y);
		return MapLighting.GetShadow(vertexNormal);
	}

	public unsafe static void AccumulateNormals(int* pIndices, Point3D* pVertices, Point3D* pNormals, int indexCount, int vertexCount)
	{
		int* ptr = pIndices + indexCount;
		while (pIndices < ptr)
		{
			Point3D point3D = Point3D.Cross(pVertices[*pIndices], pVertices[pIndices[1]], pVertices[pIndices[2]]);
			pNormals[*pIndices] += point3D;
			pNormals[pIndices[1]] += point3D;
			pNormals[pIndices[2]] += point3D;
			pIndices += 3;
		}
		Point3D* ptr2 = pNormals + vertexCount;
		while (pNormals < ptr2)
		{
			pNormals++;
		}
	}

	public static void CheckStretchTable()
	{
		if (MapLighting.m_AlwaysStretch == null)
		{
			MapLighting.m_AlwaysStretch = new bool[16384];
			MapLighting.SetAlwaysStretch(3, 167);
			MapLighting.SetAlwaysStretch(172, 301);
			MapLighting.SetAlwaysStretch(321, 427);
			MapLighting.SetAlwaysStretch(441, 499);
			MapLighting.SetAlwaysStretch(543, 585);
			MapLighting.SetAlwaysStretch(602, 1029);
			MapLighting.SetAlwaysStretch(1094, 1145);
			MapLighting.SetAlwaysStretch(1281, 1296);
			MapLighting.SetAlwaysStretch(1351, 2539);
		}
	}

	private static void SetAlwaysStretch(int start, int end)
	{
		for (int i = start; i <= end; i++)
		{
			if (Map.GetTexture(i) > 1)
			{
				MapLighting.m_AlwaysStretch[i] = true;
			}
		}
	}
}
