using System;
using System.IO;
using Ultima.Data;
using Veritas;

namespace UOAIO;

public class TileMatrix
{
	private WeakReference[][] m_Blocks;

	private Tile[] m_InvalidLandBlock;

	private HuedTile[][][] m_EmptyStaticBlock;

	private Stream m_Index;

	private BinaryReader m_IndexReader;

	private Stream m_Statics;

	private int m_BlockWidth;

	private int m_BlockHeight;

	private int m_Width;

	private int m_Height;

	private readonly int fileIndex;

	private readonly Ultima.Data.Archive archive;

	private readonly Stream m_Map;

	private readonly object m_MapSync = new object();

	private static TileList[][] m_Lists;

	public int BlockWidth => this.m_BlockWidth;

	public int BlockHeight => this.m_BlockHeight;

	public int Width => this.m_Width;

	public int Height => this.m_Height;

	public Tile[] InvalidLandBlock => this.m_InvalidLandBlock;

	public HuedTile[][][] EmptyStaticBlock => this.m_EmptyStaticBlock;

	public bool CheckLoaded(int x, int y)
	{
		return x >= 0 && x < this.m_BlockWidth && y >= 0 && y < this.m_BlockHeight && this.m_Blocks[x] != null && this.m_Blocks[x][y] != null;
	}

	private Stream Open(string name, int fileIndex)
	{
		string path = name + fileIndex + ".mul";
		Stream stream = null;
		if (!string.IsNullOrEmpty(Engine._ticket._contentArchive))
		{
			stream = Veritas.Archives.Download(Path.Combine(Engine._ticket._contentArchive, path));
		}
		if (stream == null)
		{
			string path2 = Path.Combine(Engine.FileManager.FilePath, path);
			if (File.Exists(path2))
			{
				stream = new FileStream(path2, FileMode.Open, FileAccess.Read, FileShare.Read);
			}
		}
		return stream;
	}

	public TileMatrix(int fileIndex, int mapID, int width, int height)
	{
		this.m_Width = width;
		this.m_Height = height;
		this.m_BlockWidth = width >> 3;
		this.m_BlockHeight = height >> 3;
		this.fileIndex = fileIndex;
		if (fileIndex != 127)
		{
			this.m_Index = this.Open("staidx", fileIndex);
			if (this.m_Index != null)
			{
				this.m_IndexReader = new BinaryReader(this.m_Index);
			}
			this.m_Statics = this.Open("statics", fileIndex);
		}
		this.m_EmptyStaticBlock = new HuedTile[8][][];
		for (int i = 0; i < 8; i++)
		{
			this.m_EmptyStaticBlock[i] = new HuedTile[8][];
			for (int j = 0; j < 8; j++)
			{
				this.m_EmptyStaticBlock[i][j] = new HuedTile[0];
			}
		}
		this.m_InvalidLandBlock = new Tile[196];
		this.m_Blocks = new WeakReference[this.m_BlockWidth][];
		string text = Engine.FileManager.ResolveMUL("map" + fileIndex + "LegacyMUL.uop");
		if (File.Exists(text))
		{
			this.archive = new Ultima.Data.Archive(text);
		}
		this.m_Map = ((this.archive == null) ? this.Open("map", fileIndex) : null);
	}

	public MapBlock GetBlock(int x, int y)
	{
		if (x < 0 || y < 0 || x >= this.m_BlockWidth || y >= this.m_BlockHeight)
		{
			return null;
		}
		if (this.m_Blocks[x] == null)
		{
			this.m_Blocks[x] = new WeakReference[this.m_BlockHeight];
		}
		WeakReference weakReference = this.m_Blocks[x][y];
		MapBlock result;
		if (weakReference != null)
		{
			result = (MapBlock)(weakReference.IsAlive ? ((MapBlock)weakReference.Target) : (weakReference.Target = this.LoadBlock(x, y)));
		}
		else
		{
			this.m_Blocks[x][y] = new WeakReference(result = this.LoadBlock(x, y));
		}
		return result;
	}

	public HuedTile[][][] GetStaticBlock(int x, int y)
	{
		if (x < 0 || y < 0 || x >= this.m_BlockWidth || y >= this.m_BlockHeight || this.m_Statics == null || this.m_Index == null)
		{
			return this.m_EmptyStaticBlock;
		}
		return this.GetBlock(x, y).m_StaticTiles;
	}

	public HuedTile[] GetStaticTiles(int x, int y)
	{
		HuedTile[][][] staticBlock = this.GetStaticBlock(x >> 3, y >> 3);
		return staticBlock[x & 7][y & 7];
	}

	public Tile[] GetLandBlock(int x, int y)
	{
		if (x < 0 || y < 0 || x >= this.m_BlockWidth || y >= this.m_BlockHeight)
		{
			return this.m_InvalidLandBlock;
		}
		return this.GetBlock(x, y).m_LandTiles;
	}

	public Tile GetLandTile(int x, int y)
	{
		Tile[] landBlock = this.GetLandBlock(x >> 3, y >> 3);
		return landBlock[((y & 7) << 3) + (x & 7)];
	}

	private MapBlock LoadBlock(int x, int y)
	{
		return new MapBlock(this.ReadLandBlock(x, y), this.ReadStaticBlock(x, y));
	}

	private unsafe HuedTile[][][] ReadStaticBlock(int x, int y)
	{
		BinaryReader indexReader = this.m_IndexReader;
		Stream statics = this.m_Statics;
		if (indexReader == null || statics == null)
		{
			return this.m_EmptyStaticBlock;
		}
		indexReader.BaseStream.Seek((x * this.m_BlockHeight + y) * 12, SeekOrigin.Begin);
		int num = indexReader.ReadInt32();
		int num2 = indexReader.ReadInt32();
		if (num < 0 || num2 <= 0)
		{
			return this.m_EmptyStaticBlock;
		}
		int num3 = num2 / 7;
		if (statics is FileStream file)
		{
			fixed (StaticTile* ptr = new StaticTile[num3])
			{
				statics.Seek(num, SeekOrigin.Begin);
				UnsafeMethods.ReadFile(file, ptr, num2);
				return this.GetTilesFromBlock(ptr, num3);
			}
		}
		if (statics is MemoryStream memoryStream)
		{
			fixed (byte* buffer = memoryStream.GetBuffer())
			{
				return this.GetTilesFromBlock((StaticTile*)(buffer + num), num3);
			}
		}
		throw new NotSupportedException();
	}

	private unsafe HuedTile[][][] GetTilesFromBlock(StaticTile* staticTiles, int count)
	{
		if (TileMatrix.m_Lists == null)
		{
			TileMatrix.m_Lists = new TileList[8][];
			for (int i = 0; i < 8; i++)
			{
				TileMatrix.m_Lists[i] = new TileList[8];
				for (int j = 0; j < 8; j++)
				{
					TileMatrix.m_Lists[i][j] = new TileList();
				}
			}
		}
		TileList[][] lists = TileMatrix.m_Lists;
		StaticTile* ptr = staticTiles;
		for (StaticTile* ptr2 = staticTiles + count; ptr < ptr2; ptr++)
		{
			TileList tileList = lists[ptr->x & 7][ptr->y & 7];
			tileList.Add(new HuedTile(*ptr));
		}
		HuedTile[][][] array = new HuedTile[8][][];
		for (int k = 0; k < 8; k++)
		{
			array[k] = new HuedTile[8][];
			for (int l = 0; l < 8; l++)
			{
				array[k][l] = lists[k][l].ToArray();
			}
		}
		return array;
	}

	private unsafe Tile[] ReadLandBlock(int x, int y)
	{
		int num = x * this.m_BlockHeight + y;
		if (this.archive == null)
		{
			return this.ReadLandBlockFromMul(num);
		}
		int num2 = num >> 12;
		int entry = num & 0xFFF;
		string path = $"build/map{this.fileIndex}legacymul/{num2:00000000}.dat";
		return this.archive.Open(path, delegate(Stream data)
		{
			Tile[] array = new Tile[64];
			UnmanagedMemoryStream unmanagedMemoryStream = (UnmanagedMemoryStream)data;
			fixed (Tile* target = array)
			{
				byte* positionPointer = unmanagedMemoryStream.PositionPointer;
				UnsafeMethods.CopyMemory(target, positionPointer + entry * 196 + 4, 192);
			}
			return array;
		}) ?? new Tile[64];
	}

	private unsafe Tile[] ReadLandBlockFromMul(int block)
	{
		Stream map = this.m_Map;
		if (map == null)
		{
			return new Tile[64];
		}
		byte[] array = new byte[192];
		long offset = (long)block * 196L + 4L;
		lock (this.m_MapSync)
		{
			if (offset < 0 || offset + 192L > map.Length)
			{
				return new Tile[64];
			}
			map.Seek(offset, SeekOrigin.Begin);
			int num = 0;
			while (num < 192)
			{
				int num2 = map.Read(array, num, 192 - num);
				if (num2 <= 0)
				{
					break;
				}
				num += num2;
			}
			if (num != 192)
			{
				return new Tile[64];
			}
		}
		Tile[] array2 = new Tile[64];
		fixed (byte* source = array)
		{
			fixed (Tile* target = array2)
			{
				UnsafeMethods.CopyMemory(target, source, 192);
			}
		}
		return array2;
	}

	public void Dispose()
	{
		if (this.m_Statics != null)
		{
			this.m_Statics.Close();
		}
		if (this.m_IndexReader != null)
		{
			this.m_IndexReader.Close();
		}
		if (this.m_Map != null)
		{
			this.m_Map.Close();
		}
		this.archive?.Dispose();
	}
}
