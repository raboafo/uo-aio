namespace UOAIO;

public class TileList
{
	private HuedTile[] m_Tiles;

	private int m_Count;

	private static HuedTile[] m_Empty;

	public int Count => this.m_Count;

	public TileList()
	{
		this.m_Tiles = new HuedTile[8];
		this.m_Count = 0;
	}

	public void Add(HuedTile tile)
	{
		if (this.m_Count + 1 > this.m_Tiles.Length)
		{
			HuedTile[] tiles = this.m_Tiles;
			this.m_Tiles = new HuedTile[tiles.Length * 2];
			for (int i = 0; i < tiles.Length; i++)
			{
				this.m_Tiles[i] = tiles[i];
			}
		}
		this.m_Tiles[this.m_Count++] = tile;
	}

	public HuedTile[] ToArray()
	{
		if (this.m_Count == 0)
		{
			return TileList.m_Empty;
		}
		HuedTile[] array = new HuedTile[this.m_Count];
		for (int i = 0; i < this.m_Count; i++)
		{
			array[i] = this.m_Tiles[i];
		}
		this.m_Count = 0;
		return array;
	}

	static TileList()
	{
		TileList.m_Empty = new HuedTile[0];
	}
}
