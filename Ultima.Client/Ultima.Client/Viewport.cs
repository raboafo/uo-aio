using UOAIO;

namespace Ultima.Client;

public sealed class Viewport
{
	private const int Size = 128;

	private const int Mask = 127;

	private readonly LandTile[] land;

	private int revision;

	public Viewport()
	{
		this.land = new LandTile[16384];
	}

	public void Invalidate()
	{
		this.revision++;
	}

	private static int GetSlot(int x, int y)
	{
		return (y & 0x7F) * 128 + (x & 0x7F);
	}

	public LandTile GetLandTile(int x, int y, int facet)
	{
		int slot = Viewport.GetSlot(x, y);
		LandTile landTile = this.land[slot];
		if (landTile == null || landTile.ax != x || landTile.ay != y || landTile.facet != facet || landTile.rev != this.revision)
		{
			landTile = (this.land[slot] = new LandTile());
			landTile.Prepare(facet, x, y);
			landTile.rev = this.revision;
		}
		return landTile;
	}

	public bool IsGuarded(int facet, int x, int y)
	{
		int slot = Viewport.GetSlot(x, y);
		LandTile landTile = this.land[slot];
		if (landTile != null && landTile.x == x && landTile.y == y && landTile.facet == facet)
		{
			return landTile.m_Guarded;
		}
		int averageZ = Map.GetAverageZ(x, y);
		return Region.Find(Region.GuardedRegions, x, y, averageZ, facet) != null;
	}
}
