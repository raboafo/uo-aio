using System;
using Ultima.Client.Terrain;
using UOAIO.Profiles;

namespace UOAIO;

public static class TerrainMeshProviders
{
	public static readonly TerrainMeshProvider Low;

	public static readonly TerrainMeshProvider Medium;

	public static readonly TerrainMeshProvider High;

	private static TerrainMeshProvider _current;

	public static TerrainMeshProvider Current
	{
		get
		{
			if (TerrainMeshProviders._current == null)
			{
				TerrainMeshProviders._current = TerrainMeshProviders.Acquire();
			}
			return TerrainMeshProviders._current;
		}
	}

	public static void Reset()
	{
		TerrainMeshProviders._current = null;
	}

	private static TerrainMeshProvider Acquire()
	{
		return Preferences.Current.RenderSettings.TerrainQuality switch
		{
			0 => TerrainMeshProviders.Low, 
			1 => TerrainMeshProviders.Medium, 
			2 => TerrainMeshProviders.High, 
			_ => throw new InvalidOperationException(), 
		};
	}

	public static int[] GetTriangleOffsets(TerrainMeshProvider provider, bool foldLeftRight)
	{
		return provider.GetIndices(foldLeftRight);
	}

	static TerrainMeshProviders()
	{
		TerrainMeshProviders.Low = new TerrainMeshProvider(new LowMeshProvider());
		TerrainMeshProviders.Medium = new TerrainMeshProvider(new MediumMeshProvider());
		TerrainMeshProviders.High = new TerrainMeshProvider(new HighMeshProvider());
	}
}
